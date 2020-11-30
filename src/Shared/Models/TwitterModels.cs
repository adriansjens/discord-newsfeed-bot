using System;
using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.Identity.Client;
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
        [JsonProperty("retweeted_status")]
        public object RetweetStatus { get; set; }
        public bool Truncated { get; set; }
        public Entities Entities { get; set; }
        public string Source { get; set; }
        public User User { get; set; }
        public string Lang { get; set; }
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

    public class UserList
    {
        public List<User> Data { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        [JsonProperty("id_str")]
        public long IdStr { get; set; }
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
        public string UserName { get; set; }
    }

    public class DetailedTweetList
    {
        public List<DetailedTweet> Data { get; set; }
    }

    public class DetailedTweet
    {
        public string Id { get; set; }
        [JsonProperty("author_id")]
        public string AuthorId { get; set; }
        public string Text { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        public Entities Entities { get; set; }
        [JsonProperty("referenced_tweets")]
        public List<ReferencedTweet> ReferencedTweets { get; set; }
    }

    public class ReferencedTweet
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }

    public class Entities
    {
        public List<Url> Urls { get; set; }
        [JsonProperty("user_mentions")]
        public List<UserMention> UserMentions { get; set; }
    }

    public class Url
    {
        [JsonProperty("expanded_url")]
        public Uri ExpandedUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
