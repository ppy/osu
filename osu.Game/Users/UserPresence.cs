// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Users
{
    /// <summary>
    /// Structure containing all relevant information about a user's online presence.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public struct UserPresence
    {
        /// <summary>
        /// The user's current activity.
        /// </summary>
        [Key(0)]
        public UserActivity? Activity { get; set; }

        /// <summary>
        /// The user's current status.
        /// </summary>
        [Key(1)]
        public UserStatus? Status { get; set; }
    }
}
