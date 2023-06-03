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
        public const string TAIKO_STRONG_HIT = @"taiko-strong-hit";
        public const string TAIKO_STRONG_CLAP = @"taiko-strong-clap";
        public const string TAIKO_STRONG_FLOURISH = @"taiko-strong-flourish";

        public const int SAMPLE_VOLUME_THRESHOLD_HARD = 90;
        public const int SAMPLE_VOLUME_THRESHOLD_MEDIUM = 60;

        /// <summary>
        /// A taiko-specific volume suffix, currently applied automatically based on <see cref="HitSampleInfo.Volume"/>.
        /// </summary>
        public readonly string VolumeSuffix;

        public TaikoHitSampleInfo(string name, string bank = SampleControlPoint.DEFAULT_BANK, string? suffix = null, int volume = 0)
            : base(name, bank, suffix, volume)
        {
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

                // Custom sample sets should still take priority
                if (!string.IsNullOrEmpty(Suffix))
                    lookupNames.Add($"Gameplay/{Bank}-{Name}{Suffix}");

                switch (Name)
                {
                    // Flourish shouldn't fallback to anything, it should just not play if missing
                    case TAIKO_STRONG_FLOURISH:
                        lookupNames.Add(@$"Gameplay/{Name}");

                        return lookupNames;

                    // Strongs should fallback to normals (for non-Argon skins, etc), so the hits remain audible. Playback of strongs cancels playback of the triggering
                    // normal hits (to prevent overlapping samples) - see DrumSamplePlayer
                    case TAIKO_STRONG_HIT:
                    case TAIKO_STRONG_CLAP:
                        lookupNames.Add(@$"Gameplay/{Name}");

                        if (Name == TAIKO_STRONG_HIT)
                        {
                            lookupNames.Add($"Gameplay/taiko-{Bank}-{HIT_NORMAL}");
                            lookupNames.Add($"Gameplay/{Bank}-{HIT_NORMAL}");
                        }
                        else if (Name == TAIKO_STRONG_CLAP)
                        {
                            lookupNames.Add($"Gameplay/taiko-{Bank}-{HIT_CLAP}");
                            lookupNames.Add($"Gameplay/{Bank}-{HIT_CLAP}");
                        }

                        return lookupNames;

                    case HIT_NORMAL:
                    case HIT_CLAP:
                        lookupNames.Add($"Gameplay/taiko-{Name}{VolumeSuffix}");
                        break;
                }

                lookupNames.Add($"Gameplay/taiko-{Bank}-{Name}");
                lookupNames.Add($"Gameplay/{Bank}-{Name}");
                return lookupNames;
            }
        }

        /// <inheritdoc />
        public override TaikoHitSampleInfo With(Optional<string> newName = default, Optional<string> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default)
            => new TaikoHitSampleInfo(newName.GetOr(Name), newBank.GetOr(Bank), newSuffix.GetOr(Suffix), newVolume.GetOr(Volume));

        public override int GetHashCode() => HashCode.Combine(Name, Bank, Suffix, VolumeSuffix);
    }
}
