using System.Collections.Generic;

namespace Shared.Models
{
    public class UserTweetList
    {
        public List<Tweet> Tweets { get; set; }
        public User User { get; set; }
    }

    public class Tweet
    {
        public string CreatedAt { get; set; }
        public long Id { get; set; }
        public string IdStr { get; set; }
        public string Text { get; set; }
        public bool Truncated { get; set; }
        public Entities Entities { get; set; }
        public string Source { get; set; }
        public double InReplyToStatusId { get; set; }
        public string InReplyToStatusIdStr { get; set; }
        public long InReplyToUserId { get; set; }
        public string InReplyToUserIdStr { get; set; }
        public string InReplyToScreenName { get; set; }
        public User User { get; set; }
        public string Lang { get; set; }
    }

    public class Entities
    {
        public List<UserMention> UserMentions { get; set; }
    }

    public class UserMention
    {
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public long Id { get; set; }
        public string IdStr { get; set; }
        public List<long> Indices { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        public long IdStr { get; set; }
    }
}
