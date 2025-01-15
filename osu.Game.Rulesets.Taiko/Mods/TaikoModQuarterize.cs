// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModQuarterize : Mod, IApplicableToBeatmap
    {
        public override string Name => "Quarterize";
        public override string Acronym => "QR";
        public override double ScoreMultiplier => 0.6;
        public override LocalisableString Description => "Simplify tricky rhythms!";
        public override ModType Type => ModType.DifficultyReduction;

        [SettingSource("1/3 to 1/2 conversion", "Converts 1/3 patterns to 1/2 rhythm.")]
        public Bindable<bool> OneThirdConversion { get; } = new BindableBool(false);

        [SettingSource("1/6 to 1/4 conversion", "Converts 1/6 patterns to 1/4 rhythm.")]
        public Bindable<bool> OneSixthConversion { get; } = new BindableBool(true);

        [SettingSource("1/8 to 1/4 conversion", "Converts 1/8 patterns to 1/4 rhythm.")]
        public Bindable<bool> OneEighthConversion { get; } = new BindableBool(false);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;
            var controlPointInfo = taikoBeatmap.ControlPointInfo;
            List<Hit> hits = taikoBeatmap.HitObjects.Where(obj => obj is Hit).Cast<Hit>().ToList();
            List<Hit> toRemove = new List<Hit>();

            // Snap conversions for rhythms
            var snapConversions = new Dictionary<int, double>
            {
                { 8, 4.0 }, // 1/8 snap to 1/4 snap
                { 6, 4.0 }, // 1/6 snap to 1/4 snap
                { 3, 2.0 }, // 1/3 snap to 1/2 snap
            };

            bool inPattern = false;

            foreach (var snapConversion in snapConversions)
            {
                int patternStartIndex = 0;

                // Skip processing if the corresponding conversion is disabled
                if (!shouldProcessRhythm(snapConversion.Key))
                    continue;

                for (int i = 0; i < hits.Count; i++)
                {
                    double snapValue = i < hits.Count - 1
                        ? getSnapBetweenNotes(controlPointInfo, hits[i], hits[i + 1])
                        : 1; // No next note, default to a safe 1/1 snap

                    if (snapValue == snapConversion.Key)
                    {
                        if (!inPattern)
                        {
                            patternStartIndex = i;
                        }

                        inPattern = true;
                    }

                    // Check if end of pattern
                    if (inPattern && snapValue != snapConversion.Key)
                    {
                        // End pattern
                        inPattern = false;

                        // Iterate through the pattern
                        for (int j = patternStartIndex; j <= i; j++)
                        {
                            int currentHitPosition = j - patternStartIndex;

                            if (snapConversion.Key == 8)
                            {
                                // 1/8: Remove the second note
                                if (currentHitPosition % 2 == 1)
                                {
                                    toRemove.Add(hits[j]);
                                }
                            }
                            else
                            {
                                // 1/6 and 1/3: Remove the second note and adjust the third
                                if (currentHitPosition % 3 == 1)
                                {
                                    toRemove.Add(hits[j]);
                                }
                                else if (currentHitPosition % 3 == 2 && j < hits.Count - 1)
                                {
                                    double offset = controlPointInfo.TimingPointAt(hits[j].StartTime).BeatLength / snapConversion.Value;
                                    hits[j].StartTime = hits[j + 1].StartTime - offset;
                                }
                            }
                        }
                    }
                }

                // Remove queued notes
                taikoBeatmap.HitObjects.RemoveAll(obj => toRemove.Contains(obj));
            }
        }

        private int getSnapBetweenNotes(ControlPointInfo controlPointInfo, Hit currentNote, Hit nextNote)
        {
            double gapMs = nextNote.StartTime - currentNote.StartTime;
            var currentTimingPoint = controlPointInfo.TimingPointAt(currentNote.StartTime);

            return controlPointInfo.GetClosestBeatDivisor(gapMs + currentTimingPoint.Time);
        }

        private bool shouldProcessRhythm(int snap)
        {
            return snap switch
            {
                3 => OneThirdConversion.Value,
                6 => OneSixthConversion.Value,
                8 => OneEighthConversion.Value,
                _ => false,
            };
        }
    }
}
