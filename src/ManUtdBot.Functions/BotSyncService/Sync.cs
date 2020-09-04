using System;
using System.Linq;
using System.Threading.Tasks;
using ManUtdBot.Functions.BotSyncService.FilterService;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace ManUtdBot.Functions.BotSyncService
{
    public class Sync
    {
        private readonly ISecretService _secretService;
        private readonly ITwitterApiService _twitterApiService;
        private readonly IDiscordService _discordService;

        public Sync(IServiceProvider services)
        {
            _secretService = services.GetRequiredService<ISecretService>();
            _twitterApiService = services.GetRequiredService<ITwitterApiService>();
            _discordService = services.GetRequiredService<IDiscordService>();
        }

        public async Task SyncData()
        {
            var botToken = _secretService.GetSecret("discord-bot-client-secret-manutd");
            var twitterApiToken = _secretService.GetSecret("twitter-api-token");

            if (twitterApiToken == null || botToken == null) return;

            var filterService = new BotFilterService();
            var twitterUsers = filterService.GetTwitterUsers().TwitterUsers;

            _twitterApiService.SetAuthHeader(twitterApiToken);
            var userTweetsList = await _twitterApiService.GetTimelines(twitterUsers);

            if (userTweetsList.Count == 0) return; 

            var allUsersTimelineTweets = userTweetsList.SelectMany(x => x.Tweets.Select(y => y)).ToList();

            var detailedTweets = await _twitterApiService.GetDetailedTweets(allUsersTimelineTweets.Select(x => x.Id).ToList());
            var idsOfMatchedTweets = filterService.GetMatchedTweetIds(detailedTweets);

            if (idsOfMatchedTweets.Count == 0) return;

            var result = allUsersTimelineTweets.Select(y => y).Where(x => idsOfMatchedTweets.Contains(x.Id)).ToList();
            var tweetUrls = _twitterApiService.GetTweetUrls(result);

            _discordService.SetBotAuth(botToken);
            await _discordService.SendMessagesToNewsfeed(tweetUrls);
        }
    }
}
