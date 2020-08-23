using System.Threading.Tasks;
using DSharpPlus;

namespace Shared.Services
{
    public interface IDiscordService
    {
        void InitializeClient(string token);
        Task SendMessage(ulong channelId, string message);
    }

    public class DiscordService : IDiscordService
    {
        private DiscordClient _discordClient;

        public void InitializeClient(string token)
        {
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot
            });
        }

        public async Task SendMessage(ulong channelId, string message)
        {
            if (message == null) return;


            await _discordClient.ConnectAsync();
            await _discordClient.SendMessageAsync(await _discordClient.GetChannelAsync(channelId), message);
        }
    }
    
}
