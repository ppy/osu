// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserDailyChallengeStatistics
    {
        [JsonProperty("user_id")]
        public int UserID;

        [JsonProperty("daily_streak_best")]
        public int DailyStreakBest;

        [JsonProperty("daily_streak_current")]
        public int DailyStreakCurrent;

        [JsonProperty("weekly_streak_best")]
        public int WeeklyStreakBest;

        [JsonProperty("weekly_streak_current")]
        public int WeeklyStreakCurrent;

        [JsonProperty("top_10p_placements")]
        public int Top10PercentPlacements;

        [JsonProperty("top_50p_placements")]
        public int Top50PercentPlacements;

        [JsonProperty("playcount")]
        public int PlayCount;

        [JsonProperty("last_update")]
        public DateTimeOffset? LastUpdate;

        [JsonProperty("last_weekly_streak")]
        public DateTimeOffset? LastWeeklyStreak;
    }
}
