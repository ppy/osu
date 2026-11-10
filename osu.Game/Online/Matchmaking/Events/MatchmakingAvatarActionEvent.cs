// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.Matchmaking.Events
{
    /// <summary>
    /// An action performed by a user in a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingAvatarActionEvent : MatchServerEvent
    {
        /// <summary>
        /// The user performing the action.
        /// </summary>
        [Key(0)]
        public int UserId { get; set; }

        /// <summary>
        /// The action.
        /// </summary>
        [Key(1)]
        public MatchmakingAvatarAction Action { get; set; }
    }
}
