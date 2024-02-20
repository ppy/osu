// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.Notifications.WebSocket.Events
{
    /// <summary>
    /// Reference: https://github.com/ppy/osu-web/blob/master/app/Jobs/Notifications/UserAchievementUnlock.php
    /// </summary>
    public class UserAchievementUnlock
    {
        [JsonProperty("achievement_id")]
        public uint AchievementId { get; set; }

        [JsonProperty("achievement_mode")]
        public ushort? AchievementMode { get; set; }

        [JsonProperty("cover_url")]
        public string CoverUrl { get; set; } = string.Empty;

        [JsonProperty("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("user_id")]
        public uint UserId { get; set; }
    }
}
