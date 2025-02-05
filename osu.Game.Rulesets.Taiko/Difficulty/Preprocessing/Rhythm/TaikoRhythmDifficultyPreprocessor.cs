// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;
using osu.Game.Rulesets.Taiko.Difficulty.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    public static class TaikoRhythmDifficultyPreprocessor
    {
        public static void ProcessAndAssign(List<TaikoDifficultyHitObject> hitObjects)
        {
            var rhythmGroups = createSameRhythmGroupedHitObjects(hitObjects);

            foreach (var rhythmGroup in rhythmGroups)
            {
                foreach (var hitObject in rhythmGroup.HitObjects)
                {
                    hitObject.RhythmData.SameRhythmGroupedHitObjects = rhythmGroup;
                    hitObject.HitObjectInterval = rhythmGroup.HitObjectInterval;
                }
            }

            var patternGroups = createSamePatternGroupedHitObjects(rhythmGroups);

            foreach (var patternGroup in patternGroups)
            {
                foreach (var hitObject in patternGroup.AllHitObjects)
                {
                    hitObject.RhythmData.SamePatternsGroupedHitObjects = patternGroup;
                }
            }
        }

        private static List<SameRhythmHitObjectGrouping> createSameRhythmGroupedHitObjects(List<TaikoDifficultyHitObject> hitObjects)
        {
            var rhythmGroups = new List<SameRhythmHitObjectGrouping>();
            var groups = IntervalGroupingUtils.GroupByInterval(hitObjects);

            foreach (var group in groups)
            {
                var previous = rhythmGroups.Count > 0 ? rhythmGroups[^1] : null;
                rhythmGroups.Add(new SameRhythmHitObjectGrouping(previous, group));
            }

            return rhythmGroups;
        }

        private static List<SamePatternsGroupedHitObjects> createSamePatternGroupedHitObjects(List<SameRhythmHitObjectGrouping> rhythmGroups)
        {
            var patternGroups = new List<SamePatternsGroupedHitObjects>();
            var groups = IntervalGroupingUtils.GroupByInterval(rhythmGroups);

            foreach (var group in groups)
            {
                var previous = patternGroups.Count > 0 ? patternGroups[^1] : null;
                patternGroups.Add(new SamePatternsGroupedHitObjects(previous, group));
            }

            return patternGroups;
        }
    }
}
