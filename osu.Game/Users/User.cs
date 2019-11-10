// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Users
{
    public class User
    {
        [JsonProperty(@"id")]
        public long Id = 1;

        [JsonProperty(@"join_date")]
        public DateTimeOffset JoinDate;

        [JsonProperty(@"username")]
        public string Username;

        [JsonProperty(@"previous_usernames")]
        public string[] PreviousUsernames;

        [JsonProperty(@"country")]
        public Country Country;

        public Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public IBindable<UserActivity> Activity = new Bindable<UserActivity>();

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

        [JsonProperty(@"lastfm")]
        public string Lastfm;

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

        [JsonProperty]
        private string[] playstyle
        {
            set { PlayStyles = value?.Select(str => Enum.Parse(typeof(PlayStyle), str, true)).Cast<PlayStyle>().ToArray(); }
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

        [JsonProperty(@"statistics")]
        public UserStatistics Statistics;

        public class RankHistoryData
        {
            [JsonProperty(@"mode")]
            public string Mode;

            [JsonProperty(@"data")]
            public int[] Data;
        }

        [JsonProperty(@"rankHistory")]
        private RankHistoryData rankHistory
        {
            set => Statistics.RankHistory = value;
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
    }
}
