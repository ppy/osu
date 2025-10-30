// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.Matchmaking.Events
{
    /// <summary>
    /// Requests to perform update a user's cursor position in a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingCursorPositionRequest : MatchUserRequest
    {
        /// <summary>
        /// The cursor's x position.
        /// </summary>
        [Key(0)]
        public float X { get; set; }

        /// <summary>
        /// The cursor's x position.
        /// </summary>
        [Key(1)]
        public float Y { get; set; }
    }
}
