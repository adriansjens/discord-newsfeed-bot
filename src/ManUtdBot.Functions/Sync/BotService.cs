using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace ManUtdBot.Functions.Sync
{
    public class BotService
    {
        private readonly ISecretService _secretService;
        private readonly ITwitterApiService _twitterApiService;
        private readonly IDiscordService _discordService;

        public BotService(IServiceProvider services)
        {
            _secretService = services.GetRequiredService<ISecretService>();
            _twitterApiService = services.GetRequiredService<ITwitterApiService>();
            _discordService = services.GetRequiredService<IDiscordService>();
        }

        public async Task Sync()
        {
            var botToken = _secretService.GetSecret("discord-bot-client-secret-manutd");
            var twitterApiToken = _secretService.GetSecret("twitter-api-token");

            if (twitterApiToken == null || botToken == null) return;

            var testList = new [] { "ManUtd", "FabrizioRomano", "sistoney67" };

            _twitterApiService.SetAuthHeader(twitterApiToken);
            var userTweetsList = await _twitterApiService.GetNewsfeed(testList.ToList());
            
            _discordService.InitializeClient(botToken);
            await _discordService.SendMessage(746284695859232771,
                userTweetsList.FirstOrDefault()?.Tweets.FirstOrDefault()?.Text);
        }
    }
}
