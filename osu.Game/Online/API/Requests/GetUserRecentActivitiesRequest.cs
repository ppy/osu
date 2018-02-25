using Newtonsoft.Json;
using osu.Game.Rulesets.Scoring;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Rulesets;
using osu.Game.Overlays.Profile.Sections.Recent;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRecentActivitiesRequest : APIRequest<List<RecentActivity>>
    {
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

        [JsonProperty("mode")]
        public string Mode;

        [JsonProperty("beatmap")]
        public RecentActivityBeatmap Beatmap;

        [JsonProperty("user")]
        public RecentActivityUser User;

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
        Medal,
        Rank,
        RankLost,
        UserSupportAgain,
        UserSupportFirst,
        UserSupportGift,
        UsernameChange,
    }
}
