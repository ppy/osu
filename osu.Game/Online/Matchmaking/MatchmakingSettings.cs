// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingSettings : IEquatable<MatchmakingSettings>
    {
        [Key(0)]
        public int RulesetId { get; set; }

        public bool Equals(MatchmakingSettings? other)
            => other != null && RulesetId == other.RulesetId;

        public override bool Equals(object? obj)
            => obj is MatchmakingSettings other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return RulesetId;
        }
    }
}
