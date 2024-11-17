// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class TaikoModSimplifiedRhythm : Mod, IApplicableToBeatmap
    {
        public override string Name => "Simplified Rhythm";
        public override string Acronym => "SR";
        public override double ScoreMultiplier => 0.6;
        public override LocalisableString Description => "Simplify tricky rhythms!";
        public override ModType Type => ModType.DifficultyReduction;

        [SettingSource("One-third conversion", "Converts 1/3 snap to 1/2 snap.")]
        public Bindable<bool> EnableOneThird { get; } = new BindableBool(false);

        [SettingSource("One-sixth conversion", "Converts 1/6 snap to 1/4 snap.")]
        public Bindable<bool> EnableOneSixth { get; } = new BindableBool(true);

        [SettingSource("One-eighth conversion", "Converts 1/8 snap to 1/4 snap.")]
        public Bindable<bool> EnableOneEighth { get; } = new BindableBool(false);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;
            var controlPointInfo = taikoBeatmap.ControlPointInfo;
            List<Hit> toRemove = [];

            // Snap conversions for rhythms
            var snapConversions = new Dictionary<int, int>()
            {
                { 8, 4 }, // 1/8 snap to 1/4 snap
                { 6, 4 }, // 1/6 snap to 1/4 snap
                { 3, 2 }, // 1/3 snap to 1/2 snap
            };

            double beatLength = controlPointInfo.TimingPointAt(0).BeatLength;
            int patternStartIndex = 0;
            bool inPattern = false;

            List<Hit> hits = taikoBeatmap.HitObjects.Where(obj => obj is Hit).Cast<Hit>().ToList();

            foreach (var snapConversion in snapConversions)
            {
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
                    // check if end of pattern or if we're on the last note
                    if ((inPattern && snapValue != snapConversion.Key) || i == hits.Count)
                    {
                        // End of the pattern
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
                                // 1/6 and 1/3: Adjust the second note and remove the third
                                if (currentHitPosition % 3 == 1)
                                {
                                    hits[j].StartTime = hits[j - 1].StartTime + controlPointInfo.TimingPointAt(hits[j].StartTime).BeatLength / Convert.ToDouble(snapConversion.Value);
                                }
                                else if (currentHitPosition % 3 == 2)
                                {
                                    toRemove.Add(hits[j]);
                                }
                            }
                        }
                    }
                }

                // Remove queued notes
                taikoBeatmap.HitObjects = taikoBeatmap.HitObjects.Except(toRemove).ToList();
            }
        }

        private int getSnapBetweenNotes(ControlPointInfo controlPointInfo, Hit currentNote, Hit nextNote)
        {
            double gapMs = Math.Max(currentNote.StartTime, nextNote.StartTime) - Math.Min(currentNote.StartTime, nextNote.StartTime);
            var currentTimingPoint = controlPointInfo.TimingPointAt(currentNote.StartTime);

            return controlPointInfo.GetClosestBeatDivisor(gapMs + currentTimingPoint.Time);
        }

        private bool shouldProcessRhythm(int snap)
        {
            return snap switch
            {
                3 => EnableOneThird.Value,
                6 => EnableOneSixth.Value,
                8 => EnableOneEighth.Value,
                _ => false,
            };
        }
    }
}
