// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Taiko.Audio
{
    public class TaikoHitSampleInfo : HitSampleInfo
    {
        public const string STRONG_HIT = @"strong-hit";
        public const string STRONG_CLAP = @"strong-clap";
        public const string STRONG_FLOURISH = @"strong-flourish";

        public const int SAMPLE_VOLUME_THRESHOLD_HARD = 90;
        public const int SAMPLE_VOLUME_THRESHOLD_MEDIUM = 60;

        /// <summary>
        /// A taiko-specific volume suffix, currently applied automatically based on <see cref="HitSampleInfo.Volume"/>.
        /// </summary>
        public readonly string VolumeSuffix;

        /// <summary>
        /// Whether the `taiko-` prefix has been appended to lookups.
        /// </summary>
        public readonly bool WithTaikoPrefix;

        public TaikoHitSampleInfo(string name, string bank = SampleControlPoint.DEFAULT_BANK, string? suffix = null, int volume = 0, bool withTaikoPrefix = true)
            : base(name, bank, suffix, volume)
        {
            WithTaikoPrefix = withTaikoPrefix;
            VolumeSuffix = getVolumeSuffix(name, volume);
        }

        private static string getVolumeSuffix(string name, int volume)
        {
            switch (name)
            {
                case HIT_NORMAL:
                case HIT_CLAP:
                {
                    if (volume >= SAMPLE_VOLUME_THRESHOLD_HARD)
                        return "-hard";

                    if (volume >= SAMPLE_VOLUME_THRESHOLD_MEDIUM)
                        return "-medium";

                    return "-soft";
                }

                default:
                    return string.Empty;
            }
        }

        public override IEnumerable<string> LookupNames
        {
            get
            {
                List<string> lookupNames = new List<string>();

                // Custom sample sets (with a suffix) should always take priority
                if (!string.IsNullOrEmpty(Suffix))
                    lookupNames.Add($"{Bank}-{Name}{Suffix}");

                string prefix = WithTaikoPrefix ? "Gameplay/taiko-" : "Gameplay/";

                switch (Name)
                {
                    // Flourish shouldn't fallback to anything, it should just not play if missing
                    case STRONG_FLOURISH:
                        lookupNames.Add(@$"{prefix}{Name}");
                        break;

                    // Strong hits should fallback to normals (for non-Argon skins, etc), so the hits remain audible.
                    // Playback of strong hits cancels playback of the triggering normal hits (to prevent overlapping samples) - see DrumSamplePlayer
                    case STRONG_HIT:
                        lookupNames.Add(@$"{prefix}{Name}");
                        lookupNames.Add(@$"{prefix}{Bank}-{HIT_NORMAL}");
                        break;

                    case STRONG_CLAP:
                        lookupNames.Add(@$"{prefix}{Name}");
                        lookupNames.Add(@$"{prefix}{Bank}-{HIT_CLAP}");
                        break;

                    case HIT_NORMAL:
                    case HIT_CLAP:
                        lookupNames.Add(@$"{prefix}{Name}{VolumeSuffix}");
                        lookupNames.Add(@$"{prefix}{Bank}-{Name}");
                        lookupNames.Add(@$"{prefix}{Name}");
                        break;

                    default:
                        return base.LookupNames;
                }

                return lookupNames;
            }
        }

        /// <inheritdoc />
        public override TaikoHitSampleInfo With(Optional<string> newName = default, Optional<string> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default)
            => new TaikoHitSampleInfo(newName.GetOr(Name), newBank.GetOr(Bank), newSuffix.GetOr(Suffix), newVolume.GetOr(Volume));

        public override int GetHashCode() => HashCode.Combine(Name, Bank, Suffix, VolumeSuffix);
    }
}
