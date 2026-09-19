// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingDuelIssuedParams
    {
        [Key(0)]
        public Guid Id { get; set; }

        [Key(1)]
        public int UserId { get; set; }

        [Key(2)]
        public MatchmakingPool Pool { get; set; } = new MatchmakingPool();
    }
}
