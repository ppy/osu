// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Updater
{
    public class GitHubRelease
    {
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("assets")]
        public List<GitHubAsset> Assets { get; set; }
    }
}
