using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shared.DiscordService;
using Shared.SecretService;

namespace ManUtdBot.Functions.BotService
{
    public class BotService
    {
        private readonly ISecretService _secretService;

        public BotService(IServiceProvider services)
        {
            _secretService = services.GetRequiredService<ISecretService>();
        }

        public async Task Sync()
        {
            var botToken = _secretService.GetSecret("discord-bot-client-secret-manutd");

            var discordService = new DiscordService(botToken);
        }
    }
}
