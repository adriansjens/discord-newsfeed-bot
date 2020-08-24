using System;
using System.Linq;
using System.Threading.Tasks;
using ManUtdBot.Functions.Sync.FilterService;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace ManUtdBot.Functions.Sync
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
            var userTweetsList = await _twitterApiService.GetTimeline(twitterUsers);

            var ids = userTweetsList.SelectMany(x => x.Tweets.Select(y => y.Id.ToString())).ToList();

            var detailedTweets = await _twitterApiService.GetDetailedTweets(ids);
            var result = filterService.GetMatchedTweetIds(detailedTweets);

            _discordService.SetBotAuth(botToken);
            await _discordService.SendMessage(746284695859232771,
                userTweetsList.FirstOrDefault()?.Tweets.FirstOrDefault()?.Text);
        }
    }
}
