using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Shared.Services
{
    public interface IDiscordService
    {
        void SetBotAuth(string token);
        Task SendMessagesToNewsfeed(List<string> messages);
        List<DiscordMessage> FetchExistingMessages(string channelName);
        Task SendMessageToLogChannel(List<string> messages);
        int ListGuilds();
        Task ConnectAsync();
        void DisposeClient();
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
                AutoReconnect = false
            });
        }

        public async Task SendMessagesToNewsfeed(List<string> messages)
        {
            if (messages == null) return;

            await SendMessageToLogChannel(new List<string> {$"Sending {messages.Count} messages to channels"});

            foreach (var guild in _discordClient.Guilds)
            {
                var channel = guild.Value.Channels.FirstOrDefault(x => x.Name == "latest_news");
                if (channel == null) continue;

                foreach (var message in messages)
                {
                    await _discordClient.SendMessageAsync(channel, message);
                    await Task.Delay(1500);
                }
            }
        }

        public List<DiscordMessage> FetchExistingMessages(string channelName)
        {
            var result = new List<DiscordMessage>();

            if (channelName == null) return result;

            var guild = _discordClient.Guilds.FirstOrDefault(x => x.Value.Name == "Demmeister dev server");
            var channel = guild.Value.Channels.FirstOrDefault(x => x.Name == channelName);
            if (channel == null) return result;

            result.AddRange(channel.GetMessagesAsync().Result.ToList());
            
            return result;
        }

        public async Task SendMessageToLogChannel(List<string> messages)
        {
            if (messages == null) return;

            var devLogChannel = await _discordClient.GetChannelAsync(752257665744240742);

            foreach (var message in messages)
            {
                await _discordClient.SendMessageAsync(devLogChannel, message);
            }
        }

        public async Task ConnectAsync()
        {
            await _discordClient.ConnectAsync();
        }

        public void DisposeClient()
        {
            _discordClient.Dispose();
        }

        public int ListGuilds()
        {
            return _discordClient.Guilds.Count;
        }
    }
    
}
