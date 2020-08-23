using System.Threading.Tasks;
using DSharpPlus;

namespace Shared.Services.DiscordService
{
    public class DiscordService
    {
        private DiscordClient _discordClient;

        public DiscordService(string token)
        {
            _discordClient = InitializeClient(token);
        }

        private static DiscordClient InitializeClient(string token)
        {
            return new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot
            });
        }

        public async Task SendMessage(ulong channelId, string message)
        {
            if (message == null) return;

            await _discordClient.SendMessageAsync(await _discordClient.GetChannelAsync(channelId), message);

            await _discordClient.ConnectAsync();
            await Task.Delay(-1);
        }
    }
    
}
