// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Matchmaking.Requests
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingIssueDuelRequest
    {
        [Key(0)]
        public int UserId { get; set; }

        [Key(1)]
        public int PoolId { get; set; }
    }
}
