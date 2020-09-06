using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManUtdBot.Functions.BotSyncService.FilterService;
using Microsoft.Azure.WebJobs;
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

            var detailedTweets = await _twitterApiService.GetDetailedTweets(allUsersTimelineTweets.Select(x => x.Id).ToList());
            var idsOfMatchedTweets = filterService.GetMatchedTweetIds(detailedTweets, context);

            if (!idsOfMatchedTweets.Any()) return;

            var result = allUsersTimelineTweets.Select(y => y).Where(x => idsOfMatchedTweets.Contains(x.Id)).ToList();

            var messagesToSend = new List<string>();
            foreach (var timelineTweet in result)
            {
                var (key, value) = twitterUsers.FirstOrDefault(x => x.Key == timelineTweet.User.ScreenName);

                if (value == null || key == null) continue;

                messagesToSend.Add(_twitterApiService.GetTweetUrl(timelineTweet) + " " + value);
            }

            await _discordService.SendMessagesToNewsfeed(messagesToSend);
        }
    }
}
