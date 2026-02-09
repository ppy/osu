// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayDamageInfo : IEquatable<RankedPlayDamageInfo>
    {
        /// <summary>
        /// Total amount of damage dealt.
        /// </summary>
        [Key(0)]
        public required int Damage { get; init; }

        /// <summary>
        /// Damage dealt before multipliers are applied.
        /// </summary>
        [Key(1)]
        public required int RawDamage { get; init; }

        /// <summary>
        /// Life before damage was applied.
        /// </summary>
        [Key(2)]
        public required int OldLife { get; init; }

        /// <summary>
        /// Life after damage was applied.
        /// </summary>
        [Key(3)]
        public required int NewLife { get; init; }

        public bool Equals(RankedPlayDamageInfo? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Damage == other.Damage && RawDamage == other.RawDamage && OldLife == other.OldLife && NewLife == other.NewLife;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((RankedPlayDamageInfo)obj);
        }

        public override int GetHashCode() => HashCode.Combine(Damage, RawDamage, OldLife, NewLife);
    }
}
