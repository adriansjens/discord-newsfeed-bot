using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;

namespace Shared.Services
{
    public interface IDiscordService
    {
        void SetBotAuth(string token);
        Task SendMessagesToNewsfeed(List<string> messages);
        Task SendMessageToLogChannel(List<string> messages);
    }

    public class DiscordService : IDiscordService
    {
        private DiscordClient _discordClient;

        public void SetBotAuth(string token)
        {
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            });
        }

        public async Task SendMessagesToNewsfeed(List<string> messages)
        { 
            if (messages == null) return;

            await SendMessageToLogChannel(new List<string> {$"Sending {messages.Count} messages to channels"});

            foreach (var guild in _discordClient.Guilds)
            {
                var channel = guild.Value.Channels.FirstOrDefault(x => x.Name == "twitter-newsfeed");

                if (channel == null) return;

                foreach (var message in messages)
                {
                    await _discordClient.SendMessageAsync(channel, message);
                    await Task.Delay(1500);
                }
            }
        }

        public async Task SendMessageToLogChannel(List<string> messages)
        {
            if (messages == null) return;

            var devLogChannel = await _discordClient.GetChannelAsync(751416561662951494);

            foreach (var message in messages)
            {
                await _discordClient.SendMessageAsync(devLogChannel, message);
            }

            await _discordClient.ConnectAsync();
        }
    }
    
}
