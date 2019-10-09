// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Users;
using System;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests.Responses
{
    public class Comment
    {
        [JsonProperty(@"id")]
        public long Id { get; set; }

        private long? parentId;

        [JsonProperty(@"parent_id")]
        public long? ParentId
        {
            get => parentId;
            set
            {
                parentId = value;
                IsTopLevel = value == null;
            }
        }

        public List<Comment> ChildComments = new List<Comment>();

        public Comment ParentComment { get; set; }

        [JsonProperty(@"user_id")]
        public long? UserId { get; set; }

        public User User { get; set; }

        [JsonProperty(@"message")]
        public string Message { get; set; }

        [JsonProperty(@"message_html")]
        public string MessageHTML { get; set; }

        [JsonProperty(@"replies_count")]
        public int RepliesCount { get; set; }

        [JsonProperty(@"votes_count")]
        public int VotesCount { get; set; }

        [JsonProperty(@"commenatble_type")]
        public string CommentableType { get; set; }

        [JsonProperty(@"commentable_id")]
        public int CommentableId { get; set; }

        [JsonProperty(@"legacy_name")]
        public string LegacyName { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty(@"updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        private DateTimeOffset? deletedAt;

        [JsonProperty(@"deleted_at")]
        public DateTimeOffset? DeletedAt
        {
            get => deletedAt;
            set
            {
                deletedAt = value;
                IsDeleted = value != null;
            }
        }

        [JsonProperty(@"edited_at")]
        public DateTimeOffset? EditedAt { get; set; }

        [JsonProperty(@"edited_by_id")]
        public long? EditedById { get; set; }

        public User EditedUser { get; set; }

        public bool IsTopLevel { get; set; }

        public bool IsDeleted { get; set; }

        public string GetMessage()
        {
            return MessageHTML.Replace("<div class='osu-md-default'>", "").Replace("<p class=\"osu-md-default__paragraph\">", "").Replace("<br />", "").Replace("</p>", "").Replace("</div>", "").Replace("&quot;", "\"");
        }
    }
}
