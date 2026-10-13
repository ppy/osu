// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.API;

namespace osu.Game.Online.Matchmaking.Requests
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingJoinQueueRequest
    {
        [Key(0)]
        public int PoolId { get; set; }

        [Key(1)]
        public APIMod[] Mods { get; set; } = [];
    }
}
