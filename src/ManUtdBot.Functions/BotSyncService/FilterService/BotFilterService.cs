using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ManUtdBot.Functions.Models;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Shared.Models;

namespace ManUtdBot.Functions.BotSyncService.FilterService
{
    public class BotFilterService
    {
        public List<DetailedTweet> GetMatchedTweetIds(List<DetailedTweet> detailedTweets, ExecutionContext context)
        {
            using var file = new StreamReader(context.FunctionAppDirectory +
                                        "/BotSyncService/FilterService/KeyPhrasesFilter.json");
            
            var filter = JsonConvert.DeserializeObject<FilterModel>(file.ReadToEnd());
            var filterString = $"(?i)\\b{string.Join("\\b|\\b", filter.FilterList.Concat(filter.PlayerFilterList))}\\b";

            var matches = new List<DetailedTweet>();
            foreach (var detailedTweet in detailedTweets)
            {
                var matchesFilter = Regex.Match(detailedTweet.Text, filterString);
                if (matchesFilter.Success)
                {
                    matches.Add(detailedTweet);
                }
            }

            return matches;
        }

        public Dictionary<string, string> GetTwitterUsers(ExecutionContext context)
        {
            var binDirectory = context.FunctionAppDirectory + "/BotSyncService/FilterService/TwitterUsers.json"; 
            var dict = new Dictionary<string, string>();

            using var file = new StreamReader(binDirectory);
            var twitterUserList = JsonConvert.DeserializeObject<TwitterUserList>(file.ReadToEnd());

            twitterUserList.Tier1.ForEach(x => dict.Add(x, "(Tier 1)"));
            twitterUserList.Tier2.ForEach(x => dict.Add(x, "(Tier 2)"));
            twitterUserList.Tier3.ForEach(x => dict.Add(x, "(Tier 3)"));


            return dict;
        }
    }
}
