using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ManUtdBot.Functions.Models;
using Newtonsoft.Json;
using Shared.Models;

namespace ManUtdBot.Functions.Sync.FilterService
{
    public class BotFilterService
    {
        public List<string> GetMatchedTweetIds(List<DetailedTweet> detailedTweets)
        {
            var file = new StreamReader("Sync/FilterService/KeyPhrasesFilter.json").ReadToEnd();

            var filter = JsonConvert.DeserializeObject<FilterModel>(file);

            var filterString = $"(?i)\\b{string.Join("\\b|\\b", filter.FilterList.Concat(filter.PlayerFilterList))}\\b";

            var matches = new List<string>();
            foreach (var detailedTweet in detailedTweets)
            {
                var matchesFilter = Regex.Match(detailedTweet.Text, filterString);
                if (matchesFilter.Success)
                {
                    matches.Add(detailedTweet.Id);
                }
            }

            return matches;
        }

        public TwitterUserList GetTwitterUsers()
        {
            var file = new StreamReader("Sync/FilterService/TwitterUsers.json").ReadToEnd();

            return JsonConvert.DeserializeObject<TwitterUserList>(file);
        }
    }
}
