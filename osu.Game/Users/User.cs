﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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

        //public Team Team;

        [JsonProperty(@"profile_colour")]
        public string Colour;

        [JsonProperty(@"avatar_url")]
        public string AvatarUrl;

        [JsonProperty(@"cover_url")]
        public string CoverUrl
        {
            get { return Cover?.Url; }
            set { Cover = new UserCover { Url = value }; }
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

        [JsonProperty(@"is_gmt")]
        public bool IsGMT;

        [JsonProperty(@"is_qat")]
        public bool IsQAT;

        [JsonProperty(@"is_bng")]
        public bool IsBNG;

        [JsonProperty(@"is_active")]
        public bool Active;

        [JsonProperty(@"interests")]
        public string Interests;

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
        public RankHistoryData RankHistory;

        public override string ToString() => Username;
    }
}
