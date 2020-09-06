using System.Collections.Generic;
using Newtonsoft.Json;

namespace ManUtdBot.Functions.Models
{
    public class FilterModel
    {
        public List<string> FilterList { get; set; }
        public List<string> PlayerFilterList { get; set; }
    }

    public class TwitterUserList
    {
        public List<string> Tier1 { get; set; }
        public List<string> Tier2 { get; set; }
        public List<string> Tier3 { get; set; }
        public List<string> Tier4 { get; set; }
    }
}
