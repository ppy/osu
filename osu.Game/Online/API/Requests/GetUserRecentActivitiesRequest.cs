// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.Rulesets.Scoring;
using Humanizer;
using System;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRecentActivitiesRequest : APIRequest<List<RecentActivity>>
    {
        private readonly long userId;
        private readonly int offset;

        public GetUserRecentActivitiesRequest(long userId, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;
        }

        protected override string Target => $"users/{userId}/recent_activity?offset={offset}";
    }

    public class RecentActivity
    {
        [JsonProperty("id")]
        public int ID;

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt;

        [JsonProperty]
        private string type
        {
            set => Type = (RecentActivityType)Enum.Parse(typeof(RecentActivityType), value.Pascalize());
        }

        public RecentActivityType Type;

        [JsonProperty]
        private string scoreRank
        {
            set => ScoreRank = (ScoreRank)Enum.Parse(typeof(ScoreRank), value);
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

    public enum RecentActivityType
    {
        Achievement,
        BeatmapPlaycount,
        BeatmapsetApprove,
        BeatmapsetDelete,
        BeatmapsetRevive,
        BeatmapsetUpdate,
        BeatmapsetUpload,
        Medal,
        Rank,
        RankLost,
        UserSupportAgain,
        UserSupportFirst,
        UserSupportGift,
        UsernameChange,
    }

    public enum BeatmapApproval
    {
        Ranked,
        Approved,
        Qualified,
    }
}
