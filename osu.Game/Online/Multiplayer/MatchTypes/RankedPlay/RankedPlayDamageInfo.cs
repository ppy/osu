// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Describes each source of damage.
        /// </summary>
        [Key(4)]
        public List<RankedPlayDamageSource> Sources { get; set; } = [];

        public bool Equals(RankedPlayDamageInfo? other)
        {
            if (other == null)
                return false;

            if (Damage != other.Damage || RawDamage != other.RawDamage || OldLife != other.OldLife || NewLife != other.NewLife)
                return false;

            if (Sources.Count != other.Sources.Count)
                return false;

            for (int i = 0; i < Sources.Count; i++)
            {
                if (!Sources[i].Equals(other.Sources[i]))
                    return false;
            }

            return true;
        }
    }

    [Serializable]
    [MessagePackObject]
    public class RankedPlayDamageSource : IEquatable<RankedPlayDamageSource>
    {
        /// <summary>
        /// The damage source.
        /// </summary>
        [Key(0)]
        public RankedPlayDamageType Type { get; set; }

        /// <summary>
        /// Raw value of this damage source (e.g. multiplier, score, etc).
        /// </summary>
        [Key(1)]
        public double RawValue { get; set; }

        /// <summary>
        /// Total amount of damage dealt from this source.
        /// </summary>
        [Key(2)]
        public int Damage { get; set; }

        public bool Equals(RankedPlayDamageSource? other)
        {
            if (other == null)
                return false;

            return Type == other.Type && RawValue == other.RawValue && Damage == other.Damage;
        }
    }

    public enum RankedPlayDamageType
    {
        /// <summary>
        /// Score damage dealt directly by the opposing player.
        /// </summary>
        Attack,

        /// <summary>
        /// Damage inflicted through the damage multiplier.
        /// </summary>
        Multiplier,

        /// <summary>
        /// Base damage dealt for winning a round.
        /// </summary>
        Bonus
    }
}
