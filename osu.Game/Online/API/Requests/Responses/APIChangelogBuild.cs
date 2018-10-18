// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIChangelogBuild
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("display_version")]
        public string DisplayVersion { get; set; }

        [JsonProperty("users")]
        public long Users { get; set; }

        [JsonProperty("is_featured")]
        public bool IsFeatured { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("update_stream")]
        public UpdateStream UpdateStream { get; set; }

        [JsonProperty("changelog_entries")]
        public List<ChangelogEntry> ChangelogEntries { get; set; }

        [JsonProperty("versions")]
        public Versions Versions { get; set; }
    }

    public class Versions
    {
        [JsonProperty("next")]
        public APIChangelogBuild Next { get; set; }

        [JsonProperty("previous")]
        public APIChangelogBuild Previous { get; set; }
    }

    public class ChangelogEntry
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
        public string Type { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message_html")]
        public string MessageHtml { get; set; }

        [JsonProperty("major")]
        public bool? Major { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("github_user")]
        public GithubUser GithubUser { get; set; }
    }

    public class GithubUser
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("github_url")]
        public string GithubUrl { get; set; }

        [JsonProperty("osu_username")]
        public string OsuUsername { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("user_url")]
        public string UserUrl { get; set; }
    }

    public class UpdateStream
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }
}
