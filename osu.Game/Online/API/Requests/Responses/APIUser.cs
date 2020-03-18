// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Users;
using static osu.Game.Users.User;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUser
    {
        [JsonProperty(@"id")]
        private long id { get; set; }

        [JsonProperty(@"join_date")]
        private DateTimeOffset joinDate { get; set; }

        [JsonProperty(@"username")]
        private string username { get; set; }

        [JsonProperty(@"previous_usernames")]
        private string[] previousUsernames { get; set; }

        [JsonProperty(@"country")]
        private Country country { get; set; }

        [JsonProperty(@"profile_colour")]
        private string colour { get; set; }

        [JsonProperty(@"avatar_url")]
        private string avatarUrl { get; set; }

        [JsonProperty(@"cover_url")]
        private string coverUrl
        {
            get => cover?.Url;
            set => cover = new UserCover { Url = value };
        }

        [JsonProperty(@"cover")]
        private UserCover cover { get; set; }

        [JsonProperty(@"is_admin")]
        private bool isAdmin { get; set; }

        [JsonProperty(@"is_supporter")]
        private bool isSupporter { get; set; }

        [JsonProperty(@"support_level")]
        private int supportLevel { get; set; }

        [JsonProperty(@"is_gmt")]
        private bool isGMT { get; set; }

        [JsonProperty(@"is_qat")]
        private bool isQAT { get; set; }

        [JsonProperty(@"is_bng")]
        private bool isBNG { get; set; }

        [JsonProperty(@"is_bot")]
        private bool isBot { get; set; }

        [JsonProperty(@"is_active")]
        private bool active { get; set; }

        [JsonProperty(@"is_online")]
        private bool isOnline { get; set; }

        [JsonProperty(@"pm_friends_only")]
        private bool pmFriendsOnly { get; set; }

        [JsonProperty(@"interests")]
        private string interests { get; set; }

        [JsonProperty(@"occupation")]
        private string occupation { get; set; }

        [JsonProperty(@"title")]
        private string title { get; set; }

        [JsonProperty(@"location")]
        private string location { get; set; }

        [JsonProperty(@"last_visit")]
        private DateTimeOffset? lastVisit { get; set; }

        [JsonProperty(@"twitter")]
        private string twitter { get; set; }

        [JsonProperty(@"lastfm")]
        private string lastfm { get; set; }

        [JsonProperty(@"skype")]
        private string skype { get; set; }

        [JsonProperty(@"discord")]
        private string discord { get; set; }

        [JsonProperty(@"website")]
        private string website { get; set; }

        [JsonProperty(@"post_count")]
        private int postCount { get; set; }

        [JsonProperty(@"follower_count")]
        private int followerCount { get; set; }

        [JsonProperty(@"favourite_beatmapset_count")]
        private int favouriteBeatmapsetCount { get; set; }

        [JsonProperty(@"graveyard_beatmapset_count")]
        private int graveyardBeatmapsetCount { get; set; }

        [JsonProperty(@"loved_beatmapset_count")]
        private int lovedBeatmapsetCount { get; set; }

        [JsonProperty(@"ranked_and_approved_beatmapset_count")]
        private int rankedAndApprovedBeatmapsetCount { get; set; }

        [JsonProperty(@"unranked_beatmapset_count")]
        private int unrankedBeatmapsetCount { get; set; }

        [JsonProperty(@"scores_first_count")]
        private int scoresFirstCount { get; set; }

        [JsonProperty]
        private string[] playstyle
        {
            set => playStyles = value?.Select(str => Enum.Parse(typeof(PlayStyle), str, true)).Cast<PlayStyle>().ToArray();
        }

        private PlayStyle[] playStyles { get; set; }

        [JsonProperty(@"playmode")]
        private string playMode { get; set; }

        [JsonProperty(@"profile_order")]
        private string[] profileOrder { get; set; }

        [JsonProperty(@"kudosu")]
        private KudosuCount kudosu { get; set; }

        [JsonProperty(@"statistics")]
        private UserStatistics statistics { get; set; }

        [JsonProperty(@"rankHistory")]
        private RankHistoryData rankHistory
        {
            set => statistics.RankHistory = value;
        }

        [JsonProperty("badges")]
        private Badge[] badges { get; set; }

        [JsonProperty("user_achievements")]
        private UserAchievement[] achievements { get; set; }

        [JsonProperty("monthly_playcounts")]
        private UserHistoryCount[] monthlyPlaycounts { get; set; }

        [JsonProperty("replays_watched_counts")]
        private UserHistoryCount[] replaysWatchedCounts { get; set; }

        public User ToUser() => new User
        {
            Id = id,
            JoinDate = joinDate,
            Username = username,
            PreviousUsernames = previousUsernames,
            Country = country,
            Colour = colour,
            AvatarUrl = avatarUrl,
            CoverUrl = coverUrl,
            Cover = cover,
            IsAdmin = isAdmin,
            IsSupporter = isSupporter,
            SupportLevel = supportLevel,
            IsGMT = isGMT,
            IsQAT = isQAT,
            IsBNG = isBNG,
            IsBot = isBot,
            Active = active,
            IsOnline = isOnline,
            PMFriendsOnly = pmFriendsOnly,
            Interests = interests,
            Occupation = occupation,
            Title = title,
            Location = location,
            LastVisit = lastVisit,
            Twitter = twitter,
            Lastfm = lastfm,
            Skype = skype,
            Discord = discord,
            Website = website,
            PostCount = postCount,
            FollowerCount = followerCount,
            FavouriteBeatmapsetCount = favouriteBeatmapsetCount,
            GraveyardBeatmapsetCount = graveyardBeatmapsetCount,
            LovedBeatmapsetCount = lovedBeatmapsetCount,
            RankedAndApprovedBeatmapsetCount = rankedAndApprovedBeatmapsetCount,
            UnrankedBeatmapsetCount = unrankedBeatmapsetCount,
            ScoresFirstCount = scoresFirstCount,
            PlayStyles = playStyles,
            PlayMode = playMode,
            ProfileOrder = profileOrder,
            Kudosu = kudosu,
            Statistics = statistics,
            Badges = badges,
            Achievements = achievements,
            MonthlyPlaycounts = monthlyPlaycounts,
            ReplaysWatchedCounts = replaysWatchedCounts
        };
    }
}
