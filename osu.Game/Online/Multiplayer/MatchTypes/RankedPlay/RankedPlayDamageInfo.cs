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
        public int Damage { get; set; }

        /// <summary>
        /// Damage dealt before multipliers are applied.
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

        [Key(4)]
        public int DirectDamage { get; set; }

        [Key(5)]
        public double Multiplier { get; set; } = 1;

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
