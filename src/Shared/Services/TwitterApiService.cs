using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Models;

namespace Shared.Services
{
    public interface ITwitterApiService
    {
        Task<List<UserTimeline>> GetTimelines(List<string> twitterUserList);
        Task<List<DetailedTweet>> GetDetailedTweets(List<string> ids);
        string GetTweetUrl(TimelineTweet tweet);
        void SetAuthHeader(string token);
    }

    public class TwitterApiService : ITwitterApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cache;

        public TwitterApiService(HttpClient httpClient, ICacheService cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<UserTimeline>> GetTimelines(List<string> twitterUserList)
        {
            var results = new List<UserTimeline>();
            foreach (var userId in twitterUserList)
            {
                var lastSyncedTweetId = _cache.Get($"since_id_for_{userId}");
                var endpoint = lastSyncedTweetId == null 
                    ? $"1.1/statuses/user_timeline.json?screen_name={userId}&count=10" 
                    : $"1.1/statuses/user_timeline.json?screen_name={userId}&since_id={lastSyncedTweetId}";

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode) continue;
                var tweetList =
                    JsonConvert.DeserializeObject<List<TimelineTweet>>(response.Content.ReadAsStringAsync().Result);

                if (!tweetList.Any()) continue;

                results.Add(new UserTimeline
                {
                    User = tweetList.FirstOrDefault()?.User,
                    Tweets = tweetList
                });

                _cache.Set($"since_id_for_{userId}", tweetList.FirstOrDefault()?.Id);
            }
            return results;
        }

        public async Task<List<DetailedTweet>> GetDetailedTweets(List<string> ids)
        {
            var result = new List<DetailedTweet>();

            var requestIdString = string.Join(",", ids.Take(100));
            var endpoint = $"2/tweets?ids={requestIdString}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode) return new List<DetailedTweet>();

            result.AddRange(JsonConvert
                .DeserializeObject<DetailedTweetList>(response.Content.ReadAsStringAsync().Result).Data);

            if (ids.Count <= 100) return result;

            var pagesToRequest = Math.Ceiling(ids.Count / 100.0);
            for (var i = 1; i < pagesToRequest; i++)
            {
                ids.RemoveRange(0, 100);
                var newRequestList = ids.Take(100);

                var pagingRequestIdString = string.Join(",", newRequestList);
                var pagingEndpoint = $"2/tweets?ids={pagingRequestIdString}";

                var pagingResponse = await _httpClient.GetAsync(pagingEndpoint);

                if (!response.IsSuccessStatusCode) break;

                result.AddRange(JsonConvert
                    .DeserializeObject<DetailedTweetList>(pagingResponse.Content.ReadAsStringAsync().Result).Data);
            }

            return result;
        }

        public string GetTweetUrl(TimelineTweet tweet)
        {
            return $"https://twitter.com/{tweet.User.ScreenName}/status/{tweet.Id}";
        }

        public void SetAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
