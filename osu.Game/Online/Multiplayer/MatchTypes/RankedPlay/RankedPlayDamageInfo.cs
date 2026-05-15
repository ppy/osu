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
        /// Calculated as <see cref="DirectDamage"/> * <see cref="Multiplier"/> + <see cref="BonusDamage"/>.
        /// </summary>
        [Key(0)]
        public int Damage { get; set; }

        /// <summary>
        /// Damage dealt before multipliers are applied.
        /// Calculated as <see cref="DirectDamage"/> + <see cref="BonusDamage"/>.
        /// </summary>
        [Key(1)]
        public int RawDamage { get; set; }

        /// <summary>
        /// Life before damage was applied.
        /// </summary>
        [Key(2)]
        public int OldLife { get; set; }

        /// <summary>
        /// Life after damage was applied.
        /// </summary>
        [Key(3)]
        public int NewLife { get; set; }

        /// <summary>
        /// Direct damage dealt based on score difference.
        /// </summary>
        [Key(4)]
        public int DirectDamage { get; set; }

        /// <summary>
        /// The multiplier of <see cref="DirectDamage"/>.
        /// </summary>
        [Key(5)]
        public double Multiplier { get; set; } = 1;

        /// <summary>
        /// Damage dealt for winning a round.
        /// </summary>
        [Key(6)]
        public int BonusDamage { get; set; }

        public bool Equals(RankedPlayDamageInfo? other)
        {
            if (other == null)
                return false;

            return Damage == other.Damage
                   && Damage == other.RawDamage
                   && OldLife == other.OldLife
                   && NewLife == other.NewLife
                   && DirectDamage == other.DirectDamage
                   && Multiplier == other.Multiplier
                   && BonusDamage == other.BonusDamage;
        }
    }
}
