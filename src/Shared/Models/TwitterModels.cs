using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class UserTimeline
    {
        public List<TimelineTweet> Tweets { get; set; }
        public User User { get; set; }
    }

    public class TimelineTweet
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        public string Id { get; set; }
        [JsonProperty("id_str")]
        public string IdStr { get; set; }
        public string Text { get; set; }
        public bool Truncated { get; set; }
        public Entities Entities { get; set; }
        public string Source { get; set; }
        public User User { get; set; }
        public string Lang { get; set; }
    }

    public class Entities
    {
        [JsonProperty("user_mentions")]
        public List<UserMention> UserMentions { get; set; }
    }

    public class UserMention
    {
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public long Id { get; set; }
        [JsonProperty("id_str")]
        public string IdStr { get; set; }
        public List<long> Indices { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        [JsonProperty("id_str")]
        public long IdStr { get; set; }
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
    }

    public class DetailedTweetList
    {
        public List<DetailedTweet> Data { get; set; }
    }

    public class DetailedTweet
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}
