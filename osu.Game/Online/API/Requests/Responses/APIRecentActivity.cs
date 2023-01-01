// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using Newtonsoft.Json;
using osu.Game.Extensions;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIRecentActivity
    {
        [JsonProperty("id")]
        public int ID;

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt;

        [JsonProperty]
        private string type
        {
            set => Type = Enum.Parse<RecentActivityType>(value.ToPascalCase());
        }

        public RecentActivityType Type;

        [JsonProperty]
        private string scoreRank
        {
            set => ScoreRank = Enum.Parse<ScoreRank>(value);
        }

        public ScoreRank ScoreRank;

        [JsonProperty("rank")]
        public int Rank;

        [JsonProperty("approval")]
        public BeatmapApproval Approval;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("mode")]
        public string Mode;

        [JsonProperty("beatmap")]
        public RecentActivityBeatmap Beatmap;

        [JsonProperty("beatmapset")]
        public RecentActivityBeatmap Beatmapset;

        [JsonProperty("user")]
        public RecentActivityUser User;

        [JsonProperty("achievement")]
        public RecentActivityAchievement Achievement;

        public class RecentActivityBeatmap
        {
            [JsonProperty("title")]
            public string Title;

            [JsonProperty("url")]
            public string Url;
        }

        public class RecentActivityUser
        {
            [JsonProperty("username")]
            public string Username;

            [JsonProperty("url")]
            public string Url;

            [JsonProperty("previousUsername")]
            public string PreviousUsername;
        }

        public class RecentActivityAchievement
        {
            [JsonProperty("slug")]
            public string Slug;

            [JsonProperty("name")]
            public string Name;
        }
    }
}
