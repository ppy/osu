// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModSimplifiedRhythm : Mod, IApplicableToBeatmap
    {
        public override string Name => "Simplified Rhythm";
        public override string Acronym => "SR";
        public override double ScoreMultiplier => 0.6;
        public override LocalisableString Description => "Simplify tricky rhythms!";
        public override IconUsage? Icon => OsuIcon.ModSimplifiedRhythm;
        public override ModType Type => ModType.DifficultyReduction;

        [SettingSource("1/3 to 1/2 conversion", "Converts 1/3 patterns to 1/2 rhythm.")]
        public Bindable<bool> OneThirdConversion { get; } = new BindableBool();

        [SettingSource("1/6 to 1/4 conversion", "Converts 1/6 patterns to 1/4 rhythm.")]
        public Bindable<bool> OneSixthConversion { get; } = new BindableBool(true);

        [SettingSource("1/8 to 1/4 conversion", "Converts 1/8 patterns to 1/4 rhythm.")]
        public Bindable<bool> OneEighthConversion { get; } = new BindableBool();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;
            var controlPointInfo = taikoBeatmap.ControlPointInfo;

            Hit[] hits = taikoBeatmap.HitObjects.OfType<Hit>().ToArray();

            if (hits.Length == 0)
                return;

            var conversions = new List<(int, int)>();

            if (OneEighthConversion.Value) conversions.Add((8, 4));
            if (OneSixthConversion.Value) conversions.Add((6, 4));
            if (OneThirdConversion.Value) conversions.Add((3, 2));

            bool inPattern = false;

            foreach ((int baseRhythm, int adjustedRhythm) in conversions)
            {
                int patternStartIndex = 0;

                for (int i = 1; i < hits.Length; i++)
                {
                    double snapValue = getSnapBetweenNotes(controlPointInfo, hits[i - 1], hits[i]);

                    if (inPattern)
                    {
                        // pattern continues
                        if (snapValue == baseRhythm)
                            continue;

                        inPattern = false;
                        processPattern(i);
                    }
                    else
                    {
                        if (snapValue == baseRhythm)
                        {
                            patternStartIndex = i - 1;
                            inPattern = true;
                        }
                    }
                }

                // Process the last pattern if we reached the end of the beatmap and are still in a pattern.
                if (inPattern)
                    processPattern(hits.Length);

                void processPattern(int patternEndIndex)
                {
                    // Iterate through the pattern
                    for (int j = patternStartIndex; j < patternEndIndex; j++)
                    {
                        int indexInPattern = j - patternStartIndex;

                        switch (baseRhythm)
                        {
                            // 1/8: Remove every second note
                            case 8:
                            {
                                if (indexInPattern % 2 == 1)
                                {
                                    taikoBeatmap.HitObjects.Remove(hits[j]);
                                }

                                break;
                            }

                            // 1/6 and 1/3: Remove every second note and adjust time of every third
                            case 6:
                            case 3:
                            {
                                if (indexInPattern % 3 == 1)
                                    taikoBeatmap.HitObjects.Remove(hits[j]);
                                else if (indexInPattern % 3 == 2)
                                    hits[j].StartTime = hits[j - 2].StartTime + controlPointInfo.TimingPointAt(hits[j].StartTime).BeatLength / adjustedRhythm;

                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException(nameof(baseRhythm));
                        }
                    }
                }
            }
        }

        private int getSnapBetweenNotes(ControlPointInfo controlPointInfo, Hit currentNote, Hit nextNote)
        {
            var currentTimingPoint = controlPointInfo.TimingPointAt(currentNote.StartTime);
            return controlPointInfo.GetClosestBeatDivisor(currentTimingPoint.Time + (nextNote.StartTime - currentNote.StartTime));
        }
    }
}
