// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.Matchmaking.Events
{
    /// <summary>
    /// Requests to perform an action on a user's avatar in a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingAvatarActionRequest : MatchUserRequest
    {
        /// <summary>
        /// The action.
        /// </summary>
        [Key(0)]
        public MatchmakingAvatarAction Action { get; set; }
    }
}
