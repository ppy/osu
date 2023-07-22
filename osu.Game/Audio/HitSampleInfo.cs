// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Utils;

namespace osu.Game.Audio
{
    /// <summary>
    /// Describes a gameplay hit sample.
    /// </summary>
    [Serializable]
    public class HitSampleInfo : ISampleInfo, IEquatable<HitSampleInfo>
    {
        public const string HIT_NORMAL = @"hitnormal";
        public const string HIT_WHISTLE = @"hitwhistle";
        public const string HIT_FINISH = @"hitfinish";
        public const string HIT_CLAP = @"hitclap";

        public const string BANK_NORMAL = @"normal";
        public const string BANK_SOFT = @"soft";
        public const string BANK_DRUM = @"drum";

        // new sample used exclusively by taiko for now.
        public const string HIT_FLOURISH = "hitflourish";

        // new bank used exclusively by taiko for now.
        public const string BANK_STRONG = @"strong";

        /// <summary>
        /// All valid sample addition constants.
        /// </summary>
        public static IEnumerable<string> AllAdditions => new[] { HIT_WHISTLE, HIT_FINISH, HIT_CLAP };

        /// <summary>
        /// All valid bank constants.
        /// </summary>
        public static IEnumerable<string> AllBanks => new[] { BANK_NORMAL, BANK_SOFT, BANK_DRUM };

        /// <summary>
        /// The name of the sample to load.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The bank to load the sample from.
        /// </summary>
        public readonly string Bank;

        /// <summary>
        /// An optional suffix to provide priority lookup. Falls back to non-suffixed <see cref="Name"/>.
        /// </summary>
        public readonly string? Suffix;

        /// <summary>
        /// The sample volume.
        /// </summary>
        public int Volume { get; }

        public HitSampleInfo(string name, string bank = SampleControlPoint.DEFAULT_BANK, string? suffix = null, int volume = 100)
        {
            Name = name;
            Bank = bank;
            Suffix = suffix;
            Volume = volume;
        }

        /// <summary>
        /// Retrieve all possible filenames that can be used as a source, returned in order of preference (highest first).
        /// </summary>
        [JsonIgnore]
        public virtual IEnumerable<string> LookupNames
        {
            get
            {
                if (!string.IsNullOrEmpty(Suffix))
                    yield return $"Gameplay/{Bank}-{Name}{Suffix}";

                yield return $"Gameplay/{Bank}-{Name}";
            }
        }

        /// <summary>
        /// Creates a new <see cref="HitSampleInfo"/> with overridden values.
        /// </summary>
        /// <param name="newName">An optional new sample name.</param>
        /// <param name="newBank">An optional new sample bank.</param>
        /// <param name="newSuffix">An optional new lookup suffix.</param>
        /// <param name="newVolume">An optional new volume.</param>
        /// <returns>The new <see cref="HitSampleInfo"/>.</returns>
        public virtual HitSampleInfo With(Optional<string> newName = default, Optional<string> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default)
            => new HitSampleInfo(newName.GetOr(Name), newBank.GetOr(Bank), newSuffix.GetOr(Suffix), newVolume.GetOr(Volume));

        public bool Equals(HitSampleInfo? other)
            => other != null && Name == other.Name && Bank == other.Bank && Suffix == other.Suffix;

        public override bool Equals(object? obj)
            => obj is HitSampleInfo other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Name, Bank, Suffix);
    }
}
