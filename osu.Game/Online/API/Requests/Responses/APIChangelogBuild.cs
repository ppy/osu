// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIChangelogBuild : IEquatable<APIChangelogBuild>
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("display_version")]
        public string DisplayVersion { get; set; }

        [JsonProperty("users")]
        public long Users { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("update_stream")]
        public APIUpdateStream UpdateStream { get; set; }

        [JsonProperty("changelog_entries")]
        public List<APIChangelogEntry> ChangelogEntries { get; set; }

        [JsonProperty("versions")]
        public VersionNavigation Versions { get; set; }

        public string Url => $"https://osu.ppy.sh/home/changelog/{UpdateStream.Name}/{Version}";

        public class VersionNavigation
        {
            [JsonProperty("next")]
            public APIChangelogBuild Next { get; set; }

            [JsonProperty("previous")]
            public APIChangelogBuild Previous { get; set; }
        }

        public bool Equals(APIChangelogBuild other) => Id == other?.Id;

        public override string ToString() => $"{UpdateStream.DisplayName} {DisplayVersion}";
    }
}
