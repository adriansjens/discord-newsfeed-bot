using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Shared.Models;

namespace Shared.Services
{
    public interface ITwitterApiService
    {
        Task<List<UserTweetList>> GetNewsfeed(List<string> twitterUserList);
        void SetAuthHeader(string token);
    }

    public class TwitterApiService : ITwitterApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;

        public TwitterApiService(HttpClient httpClient, IDistributedCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<UserTweetList>> GetNewsfeed(List<string> twitterUserList)
        {
            var results = new List<UserTweetList>();
            foreach (var userId in twitterUserList)
            {
                var lastSyncedTweetId = await _cache.GetStringAsync($"since_id_for_{userId}");
                var endpoint = lastSyncedTweetId == null 
                    ? $"statuses/user_timeline.json?screen_name={userId}&count=20" 
                    : $"statuses/user_timeline.json?screen_name={userId}&since_id={lastSyncedTweetId}";

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode) continue;
                var tweetList =
                    JsonConvert.DeserializeObject<List<Tweet>>(response.Content.ReadAsStringAsync().Result);

                results.Add(new UserTweetList
                {
                    User = tweetList.FirstOrDefault()?.User,
                    Tweets = tweetList
                });

                await _cache.SetStringAsync($"since_id_for_{userId}", tweetList.FirstOrDefault()?.Id.ToString());
            }

            return results;
        }

        public void SetAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
