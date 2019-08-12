// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIChangelogUser
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
}
