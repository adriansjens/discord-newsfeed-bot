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

            _discordClient.GuildAvailable += async r =>
            {
                foreach (var server in r.Client.Guilds)
                {
                    var channel = server.Value.Channels.FirstOrDefault(x => x.Name == "twitter-newsfeed");
                    if (channel == null) continue;

                    foreach (var message in messages)
                    {
                        await _discordClient.SendMessageAsync(channel, message);
                    }
                }
            };

            await _discordClient.ConnectAsync();
            await Task.Delay(-1);
        }
    }
    
}
