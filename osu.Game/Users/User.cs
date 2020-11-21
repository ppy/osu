// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Users
{
    public class User : IEquatable<User>
    {
        [JsonProperty(@"id")]
        public int Id = 1;

        [JsonProperty(@"join_date")]
        public DateTimeOffset JoinDate;

        [JsonProperty(@"username")]
        public string Username;

        [JsonProperty(@"previous_usernames")]
        public string[] PreviousUsernames;

        [JsonProperty(@"country")]
        public Country Country;

        public readonly Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public readonly Bindable<UserActivity> Activity = new Bindable<UserActivity>();

        //public Team Team;

        [JsonProperty(@"profile_colour")]
        public string Colour;

        [JsonProperty(@"avatar_url")]
        public string AvatarUrl;

        [JsonProperty(@"cover_url")]
        public string CoverUrl
        {
            get => Cover?.Url;
            set => Cover = new UserCover { Url = value };
        }

        [JsonProperty(@"cover")]
        public UserCover Cover;

        public class UserCover
        {
            [JsonProperty(@"custom_url")]
            public string CustomUrl;

            [JsonProperty(@"url")]
            public string Url;

            [JsonProperty(@"id")]
            public int? Id;
        }

        [JsonProperty(@"is_admin")]
        public bool IsAdmin;

        [JsonProperty(@"is_supporter")]
        public bool IsSupporter;

        [JsonProperty(@"support_level")]
        public int SupportLevel;

        [JsonProperty(@"current_mode_rank")]
        public int? CurrentModeRank;

        [JsonProperty(@"is_gmt")]
        public bool IsGMT;

        [JsonProperty(@"is_qat")]
        public bool IsQAT;

        [JsonProperty(@"is_bng")]
        public bool IsBNG;

        [JsonProperty(@"is_bot")]
        public bool IsBot;

        [JsonProperty(@"is_active")]
        public bool Active;

        [JsonProperty(@"is_online")]
        public bool IsOnline;

        [JsonProperty(@"pm_friends_only")]
        public bool PMFriendsOnly;

        [JsonProperty(@"interests")]
        public string Interests;

        [JsonProperty(@"occupation")]
        public string Occupation;

        [JsonProperty(@"title")]
        public string Title;

        [JsonProperty(@"location")]
        public string Location;

        [JsonProperty(@"last_visit")]
        public DateTimeOffset? LastVisit;

        [JsonProperty(@"twitter")]
        public string Twitter;

        [JsonProperty(@"skype")]
        public string Skype;

        [JsonProperty(@"discord")]
        public string Discord;

        [JsonProperty(@"website")]
        public string Website;

        [JsonProperty(@"post_count")]
        public int PostCount;

        [JsonProperty(@"follower_count")]
        public int FollowerCount;

        [JsonProperty(@"favourite_beatmapset_count")]
        public int FavouriteBeatmapsetCount;

        [JsonProperty(@"graveyard_beatmapset_count")]
        public int GraveyardBeatmapsetCount;

        [JsonProperty(@"loved_beatmapset_count")]
        public int LovedBeatmapsetCount;

        [JsonProperty(@"ranked_and_approved_beatmapset_count")]
        public int RankedAndApprovedBeatmapsetCount;

        [JsonProperty(@"unranked_beatmapset_count")]
        public int UnrankedBeatmapsetCount;

        [JsonProperty(@"scores_first_count")]
        public int ScoresFirstCount;

        [JsonProperty(@"beatmap_playcounts_count")]
        public int BeatmapPlaycountsCount;

        [JsonProperty]
        private string[] playstyle
        {
            set => PlayStyles = value?.Select(str => Enum.Parse(typeof(PlayStyle), str, true)).Cast<PlayStyle>().ToArray();
        }

        public PlayStyle[] PlayStyles;

        [JsonProperty(@"playmode")]
        public string PlayMode;

        [JsonProperty(@"profile_order")]
        public string[] ProfileOrder;

        [JsonProperty(@"kudosu")]
        public KudosuCount Kudosu;

        public class KudosuCount
        {
            [JsonProperty(@"total")]
            public int Total;

            [JsonProperty(@"available")]
            public int Available;
        }

        private UserStatistics statistics;

        [JsonProperty(@"statistics")]
        public UserStatistics Statistics
        {
            get => statistics ??= new UserStatistics();
            set
            {
                if (statistics != null)
                    // we may already have rank history populated
                    value.RankHistory = statistics.RankHistory;

                statistics = value;
            }
        }

        [JsonProperty(@"rank_history")]
        private RankHistoryData rankHistory
        {
            set => statistics.RankHistory = value;
        }

        public class RankHistoryData
        {
            [JsonProperty(@"mode")]
            public string Mode;

            [JsonProperty(@"data")]
            public int[] Data;
        }

        [JsonProperty("badges")]
        public Badge[] Badges;

        [JsonProperty("user_achievements")]
        public UserAchievement[] Achievements;

        public class UserAchievement
        {
            [JsonProperty("achieved_at")]
            public DateTimeOffset AchievedAt;

            [JsonProperty("achievement_id")]
            public int ID;
        }

        [JsonProperty("monthly_playcounts")]
        public UserHistoryCount[] MonthlyPlaycounts;

        [JsonProperty("replays_watched_counts")]
        public UserHistoryCount[] ReplaysWatchedCounts;

        public class UserHistoryCount
        {
            [JsonProperty("start_date")]
            public DateTime Date;

            [JsonProperty("count")]
            public long Count;
        }

        public override string ToString() => Username;

        /// <summary>
        /// A user instance for displaying locally created system messages.
        /// </summary>
        public static readonly User SYSTEM_USER = new User
        {
            Username = "system",
            Colour = @"9c0101",
            Id = 0
        };

        public enum PlayStyle
        {
            [Description("Keyboard")]
            Keyboard,

            [Description("Mouse")]
            Mouse,

            [Description("Tablet")]
            Tablet,

            [Description("Touch Screen")]
            Touch,
        }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id == other.Id;
        }
    }
}
