using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManUtdBot.Functions.BotSyncService.FilterService;
using Microsoft.Azure.WebJobs;
using Shared.Models;
using Shared.Services;

namespace ManUtdBot.Functions.BotSyncService
{
    public class Sync
    {
        private readonly ISecretService _secretService;
        private readonly ITwitterApiService _twitterApiService;
        private readonly IDiscordService _discordService;

        public Sync(ITwitterApiService twitterApiService, ISecretService secretService, 
            IDiscordService discordService)
        {
            _secretService = secretService;
            _twitterApiService = twitterApiService;
            _discordService = discordService;
        }

        public async Task SyncData(ExecutionContext context)
        {
            var twitterApiToken = _secretService.GetSecret("twitter-api-token");
            if (twitterApiToken == null) return;

            var filterService = new BotFilterService();
            var twitterUsers = filterService.GetTwitterUsers(context);

            _twitterApiService.SetAuthHeader(twitterApiToken);

            var userTweetsList = await _twitterApiService.GetTimelines(twitterUsers.Keys.ToList());
            if (!userTweetsList.Any()) return;

            var allUsersTimelineTweets = userTweetsList.SelectMany(x => x.Tweets.Select(y => y)).ToList();

            var detailedTweets =
                await _twitterApiService.GetDetailedTweets(allUsersTimelineTweets.Select(x => x.Id).ToList(), true);
            var unitedRelatedTweets = filterService.GetMatchedTweetIds(detailedTweets, context);

            await _discordService.SendMessageToLogChannel(new List<string>
                {$"Found {detailedTweets.Count} new tweets. Found {unitedRelatedTweets.Count} united-related tweets"});

            if (!unitedRelatedTweets.Any()) return;

            var filteredResult =
                await FilterTweetsWithExistingEmbeds(unitedRelatedTweets);

            if (!filteredResult.Any()) return;

            var result = allUsersTimelineTweets
                .Select(y => y)
                .Where(x => filteredResult.Select(y => y.Id).Contains(x.Id))
                .ToList();

            var messagesToSend = BuildMessageList(result, twitterUsers);

            await _discordService.SendMessagesToNewsfeed(messagesToSend);
        }

        private List<string> BuildMessageList(List<TimelineTweet> tweetsToSend, Dictionary<string, string> twitterUsers)
        {
            var messagesToSend = new List<string>();
            foreach (var timelineTweet in tweetsToSend)
            {
                var (key, value) = twitterUsers.FirstOrDefault(x => x.Key == timelineTweet.User.ScreenName);

                if (value == null || key == null) continue;
                if (timelineTweet.RetweetStatus != null) value += " " + "(Retweet)";
                messagesToSend.Add(_twitterApiService.GetTweetUrl(timelineTweet) + " " + value);
            }

            return messagesToSend;
        }

        private async Task<List<DetailedTweet>> FilterTweetsWithExistingEmbeds(List<DetailedTweet> tweets)
        {
            var existingMessagesInChannel = _discordService.FetchExistingMessages("latest_news");

            var idsForExistingTweets = existingMessagesInChannel
                .SelectMany(messages => messages.Embeds
                .Where(embed => embed.Url.ToString().Contains("twitter.com") && embed.Url.ToString().Contains("status"))
                .Select(embed => embed.Url.Segments.LastOrDefault()))
                .ToList();

            var existingTweetsInChannel = await _twitterApiService.GetDetailedTweets(idsForExistingTweets);
            var retweets = await _twitterApiService.GetDetailedTweets(existingTweetsInChannel.Where(x => x.ReferencedTweets != null)
                .SelectMany(y => y.ReferencedTweets.Select(references => references.Id)).ToList());

            var tweetsWithAddedUrls = new List<DetailedTweet>(existingTweetsInChannel.Where(x => x.ReferencedTweets == null));
            foreach (var detailedTweet in existingTweetsInChannel.Where(x => x.ReferencedTweets != null))
            {
                var newTweet = AddUrlsFromRetweets(detailedTweet, retweets);
                if (newTweet != null) tweetsWithAddedUrls.Add(newTweet);
            }

            return await FindDuplicateUrls(tweets, retweets, tweetsWithAddedUrls);
        }

        private async Task<List<DetailedTweet>> FindDuplicateUrls(List<DetailedTweet> tweets,
            List<DetailedTweet> retweets, List<DetailedTweet> tweetsWithAddedUrls)
        {
            var result = new List<DetailedTweet>();
            foreach (var detailedTweet in tweets)
            {
                var addUrlFromRetweets = AddUrlsFromRetweets(detailedTweet, retweets);
                if (addUrlFromRetweets.Entities?.Urls == null || !addUrlFromRetweets.Entities.Urls.Any())
                {
                    result.Add(addUrlFromRetweets);
                    continue;
                }

                var tweetUrls = addUrlFromRetweets.Entities.Urls
                    .Select(x => x.ExpandedUrl.ToString())
                    .Distinct()
                    .ToList();

                var tweetsWithExistingUrls = tweetsWithAddedUrls
                    .Where(x => x.Entities?.Urls != null && x.Entities.Urls.Select(y => y.ExpandedUrl.ToString())
                    .Any(value => tweetUrls.Contains(value)))
                    .ToList();

                if (!tweetsWithExistingUrls.Any())
                {
                    result.Add(detailedTweet);
                    continue; 
                }

                var currentPosterIsHigherTier =
                    await CurrentPosterIsHigherTier(detailedTweet, tweetsWithExistingUrls);

                if (currentPosterIsHigherTier) result.Add(detailedTweet);
            }

            return result;
        }

        private DetailedTweet AddUrlsFromRetweets(DetailedTweet detailedTweet, List<DetailedTweet> retweets)
        {
            var referencedTweetIds = detailedTweet.ReferencedTweets?
                .Select(x => x.Id)
                .ToList();

            var referencedTweets = GetTweetsWithUrls(retweets
                .Where(x => referencedTweetIds != null && referencedTweetIds.Contains(x.Id))
                .ToList());
            var referencedTweetsUrls = referencedTweets
                .SelectMany(x => x.Entities.Urls)
                .Distinct()
                .ToList();

            if (!referencedTweetsUrls.Any())
            {
                return detailedTweet;
            }

            if (detailedTweet.Entities == null)
                detailedTweet.Entities = new Entities {Urls = referencedTweetsUrls};
            else if (detailedTweet.Entities.Urls == null)
            {
                detailedTweet.Entities.Urls = new List<Url>(referencedTweetsUrls);
            }
            else
            {
                detailedTweet.Entities.Urls.AddRange(referencedTweetsUrls);
            }

            return detailedTweet;
        }

        private async Task<bool> CurrentPosterIsHigherTier(DetailedTweet tweetBeingPosted, List<DetailedTweet> tweetsWithExistingUrls)
        {
            await _discordService.SendMessageToLogChannel(new List<string> { "Man Utd bot: Filtering duplicate Url" });

            var currentPoster = await _twitterApiService.GetUsersById(new List<string> {tweetBeingPosted.AuthorId});
            if (currentPoster == null) return true;

            var currentPosterTier = TierChecker.CheckTier(currentPoster.Data.FirstOrDefault()?.UserName);

            var originalPosters =
                await _twitterApiService.GetUsersById(tweetsWithExistingUrls.Select(x => x.AuthorId).ToList());
            if (!originalPosters.Data.Any()) return true;

            var originalPosterTiers = originalPosters.Data?.Select(x => TierChecker.CheckTier(x.UserName)).ToList();

            var currentPosterIsHigherTier = originalPosterTiers.All(x => x < currentPosterTier);

            return currentPosterIsHigherTier;
        }

        private List<DetailedTweet> GetTweetsWithUrls(List<DetailedTweet> tweets)
        {
            var existingTweetsWithUrls = new List<DetailedTweet>();
            foreach (var detailedTweet in tweets)
            {
                var entities = detailedTweet.Entities;
                if (entities?.Urls == null) continue;
                var urls = entities.Urls;

                foreach (var url in urls)
                {
                    if (url.ExpandedUrl == null) continue;
                    existingTweetsWithUrls.Add(detailedTweet);
                }
            }

            return existingTweetsWithUrls;
        }
    }
}
