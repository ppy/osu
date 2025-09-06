// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingPool : IEquatable<MatchmakingPool>
    {
        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public int RulesetId { get; set; }

        [Key(2)]
        public int Variant { get; set; }

        [Key(3)]
        public string Name { get; set; } = string.Empty;

        public bool Equals(MatchmakingPool? other)
            => other != null
               && Id == other.Id
               && RulesetId == other.RulesetId
               && Variant == other.Variant
               && Name == other.Name;

        public override bool Equals(object? obj)
            => obj is MatchmakingPool other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() => HashCode.Combine(Id, RulesetId, Variant, Name);
    }
}
