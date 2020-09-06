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
        public List<string> GetMatchedTweetIds(List<DetailedTweet> detailedTweets, ExecutionContext context)
        {
            var file = new StreamReader(context.FunctionAppDirectory +
                                        "/BotSyncService/FilterService/KeyPhrasesFilter.json").ReadToEnd();

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

        public Dictionary<string, string> GetTwitterUsers(ExecutionContext context)
        {
            var binDirectory = context.FunctionAppDirectory + "/BotSyncService/FilterService/TwitterUsers.json";
            var file = new StreamReader(binDirectory).ReadToEnd();

            var twitterUserList = JsonConvert.DeserializeObject<TwitterUserList>(file);
            var dict = new Dictionary<string, string>();

            twitterUserList.Tier1.ForEach(x => dict.Add(x, "(Tier 1)"));
            twitterUserList.Tier2.ForEach(x => dict.Add(x, "(Tier 2)"));
            twitterUserList.Tier3.ForEach(x => dict.Add(x, "(Tier 3)"));
            twitterUserList.Tier4.ForEach(x => dict.Add(x, "(Tier 4)"));

            return dict;
        }
    }
}
