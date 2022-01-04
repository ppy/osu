// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using System;

namespace osu.Game.Online.API.Requests.Responses
{
    public class Comment
    {
        [JsonProperty(@"id")]
        public long Id { get; set; }

        [JsonProperty(@"parent_id")]
        public long? ParentId { get; set; }

        public Comment ParentComment { get; set; }

        [JsonProperty(@"user_id")]
        public long? UserId { get; set; }

        public APIUser User { get; set; }

        [JsonProperty(@"message")]
        public string Message { get; set; }

        [JsonProperty(@"message_html")]
        public string MessageHtml { get; set; }

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

        [JsonProperty(@"deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [JsonProperty(@"edited_at")]
        public DateTimeOffset? EditedAt { get; set; }

        [JsonProperty(@"edited_by_id")]
        public long? EditedById { get; set; }

        [JsonProperty(@"pinned")]
        public bool Pinned { get; set; }

        public APIUser EditedUser { get; set; }

        public bool IsTopLevel => !ParentId.HasValue;

        public bool IsDeleted => DeletedAt.HasValue;

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public bool IsVoted { get; set; }
    }
}
