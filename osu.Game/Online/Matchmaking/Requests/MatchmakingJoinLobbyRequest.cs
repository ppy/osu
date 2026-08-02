// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Matchmaking.Requests
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingJoinLobbyRequest
    {
        /// <summary>
        /// The pool to receive status updates from.
        /// </summary>
        [Key(0)]
        public int PoolId { get; set; }
    }
}
