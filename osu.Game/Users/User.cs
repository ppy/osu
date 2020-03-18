// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Users
{
    public class User : IEquatable<User>
    {
        public long Id { get; set; } = 1;

        public DateTimeOffset JoinDate { get; set; }

        public string Username { get; set; }

        public string[] PreviousUsernames { get; set; }

        public Country Country { get; set; }

        public readonly Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public readonly Bindable<UserActivity> Activity = new Bindable<UserActivity>();

        public string Colour { get; set; }

        public string AvatarUrl { get; set; }

        public string CoverUrl
        {
            get => Cover?.Url;
            set => Cover = new UserCover { Url = value };
        }

        public UserCover Cover { get; set; }

        public class UserCover
        {
            [JsonProperty(@"custom_url")]
            public string CustomUrl;

            [JsonProperty(@"url")]
            public string Url;

            [JsonProperty(@"id")]
            public int? Id;
        }

        public bool IsAdmin { get; set; }

        public bool IsSupporter { get; set; }

        public int SupportLevel { get; set; }

        public bool IsGMT { get; set; }

        public bool IsQAT { get; set; }

        public bool IsBNG { get; set; }

        public bool IsBot { get; set; }

        public bool Active { get; set; }

        public bool IsOnline { get; set; }

        public bool PMFriendsOnly { get; set; }

        public string Interests { get; set; }

        public string Occupation { get; set; }

        public string Title { get; set; }

        public string Location { get; set; }

        public DateTimeOffset? LastVisit { get; set; }

        public string Twitter { get; set; }

        public string Lastfm { get; set; }

        public string Skype { get; set; }

        public string Discord { get; set; }

        public string Website { get; set; }

        public int PostCount { get; set; }

        public int FollowerCount { get; set; }

        public int FavouriteBeatmapsetCount { get; set; }

        public int GraveyardBeatmapsetCount { get; set; }

        public int LovedBeatmapsetCount { get; set; }

        public int RankedAndApprovedBeatmapsetCount { get; set; }

        public int UnrankedBeatmapsetCount { get; set; }

        public int ScoresFirstCount { get; set; }

        public PlayStyle[] PlayStyles { get; set; }

        public string PlayMode { get; set; }

        public string[] ProfileOrder { get; set; }

        public KudosuCount Kudosu { get; set; }

        public class KudosuCount
        {
            [JsonProperty(@"total")]
            public int Total;

            [JsonProperty(@"available")]
            public int Available;
        }

        public UserStatistics Statistics { get; set; }

        public class RankHistoryData
        {
            [JsonProperty(@"mode")]
            public string Mode;

            [JsonProperty(@"data")]
            public int[] Data;
        }

        public Badge[] Badges { get; set; }

        public UserAchievement[] Achievements { get; set; }

        public class UserAchievement
        {
            [JsonProperty("achieved_at")]
            public DateTimeOffset AchievedAt;

            [JsonProperty("achievement_id")]
            public int ID;
        }

        public UserHistoryCount[] MonthlyPlaycounts { get; set; }

        public UserHistoryCount[] ReplaysWatchedCounts { get; set; }

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
