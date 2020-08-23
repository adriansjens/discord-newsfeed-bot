using System.Collections.Generic;
using System.Net.Http;

namespace Shared.Services.TwitterApiService
{
    public class TwitterApiService
    {
        private readonly string _apiToken;
        private readonly HttpClient _httpClient;

        public TwitterApiService(string token, HttpClient httpClient)
        {
            _apiToken = token;
            _httpClient = httpClient;
        }

        public List<string> GetNewsfeed(List<string> twitterUserList)
        {

            return null;
        }
    }
}
