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

        [JsonProperty("created_at")]
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

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("mode")]
        public string Mode;

        [JsonProperty("beatmap")]
        public RecentActivityBeatmap Beatmap;

        [JsonProperty("user")]
        public RecentActivityUser User;

        [JsonProperty("achivementName")]
        public string AchivementName;

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
}
