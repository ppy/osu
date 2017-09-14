// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
using osu.Framework.Configuration;

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

        [JsonProperty(@"country")]
        public Country Country;

        public Bindable<UserStatus> Status = new Bindable<UserStatus>();

        [JsonProperty(@"age")]
        public int? Age;

        public int GlobalRank;

        public int CountryRank;

        //public Team Team;

        [JsonProperty(@"profile_colour")]
        public string Colour;

        [JsonProperty(@"avatar_url")]
        public string AvatarUrl;

        [JsonProperty(@"cover_url")]
        public string CoverUrl;

        //[JsonProperty(@"cover")]
        //public UserCover Cover;

        public class UserCover
        {
            [JsonProperty(@"custom_url")]
            public string CustomUrl;

            [JsonProperty(@"url")]
            public string Url;

            [JsonProperty(@"id")]
            public int? Id;
        }

        [JsonProperty(@"isAdmin")]
        public bool IsAdmin;

        [JsonProperty(@"isSupporter")]
        public bool IsSupporter;

        [JsonProperty(@"isGMT")]
        public bool IsGMT;

        [JsonProperty(@"isQAT")]
        public bool IsQAT;

        [JsonProperty(@"isBNG")]
        public bool IsBNG;

        [JsonProperty(@"is_active")]
        public bool Active;

        [JsonProperty(@"interests")]
        public string Intrerests;

        [JsonProperty(@"occupation")]
        public string Occupation;

        [JsonProperty(@"title")]
        public string Title;

        [JsonProperty(@"location")]
        public string Location;

        [JsonProperty(@"lastvisit")]
        public DateTimeOffset LastVisit;

        [JsonProperty(@"twitter")]
        public string Twitter;

        [JsonProperty(@"lastfm")]
        public string Lastfm;

        [JsonProperty(@"skype")]
        public string Skype;

        [JsonProperty(@"website")]
        public string Website;

        [JsonProperty(@"playstyle")]
        public string[] PlayStyle;

        [JsonProperty(@"playmode")]
        public string PlayMode;

        [JsonProperty(@"profileOrder")]
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
        public RankHistoryData RankHistory;
    }
}
