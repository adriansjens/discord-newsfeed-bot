using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Services;

namespace ManUtdBot.Functions
{
    public class ManUtdBot
    {
        private readonly IServiceProvider _services;

        public ManUtdBot(IServiceProvider services)
        { 
            _services = services;
        }

        [FunctionName("SyncNewsfeed")]
        public async Task SyncNewsfeed([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log, 
            ExecutionContext context)
        {
            var secretService = _services.GetRequiredService<ISecretService>();
            var botToken = secretService.GetSecret("discord-bot-client-secret-manutd");

            var discordService = _services.GetRequiredService<IDiscordService>();
            discordService.SetBotAuth(botToken);

            try
            {
                await discordService.SendMessageToLogChannel(new List<string> {"Executing SyncNewsfeed for ManUtdBot"});

                var twitterService = _services.GetRequiredService<ITwitterApiService>();
                var botService = new BotSyncService.Sync(twitterService, secretService, discordService);

                await botService.SyncData(context);
            }

            catch (Exception e)
            {
                await discordService.SendMessageToLogChannel(
                    new List<string> {"Sync newsfeed", e.StackTrace, e.Message});
            }
        }
    }
}