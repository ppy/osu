// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIChangelogEntry
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("repository")]
        public string Repository { get; set; }

        [JsonProperty("github_pull_request_id")]
        public long? GithubPullRequestId { get; set; }

        [JsonProperty("github_url")]
        public string GithubUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public ChangelogEntryType Type { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message_html")]
        public string MessageHtml { get; set; }

        [JsonProperty("major")]
        public bool Major { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("github_user")]
        public APIChangelogUser GithubUser { get; set; }
    }

    public enum ChangelogEntryType
    {
        Add,
        Fix
    }
}
