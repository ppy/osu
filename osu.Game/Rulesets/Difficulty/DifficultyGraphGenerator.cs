// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Difficulty
{
    public class DifficultyGraphGenerator
    {
        private readonly Ruleset ruleset;
        private readonly WorkingBeatmap beatmap;
        private readonly DifficultyCalculator difficultyCalculator;

        public DifficultyGraphGenerator(Ruleset ruleset, WorkingBeatmap beatmap, DifficultyCalculator difficultyCalculator)
        {
            this.ruleset = ruleset;
            this.beatmap = beatmap;
            this.difficultyCalculator = difficultyCalculator;
        }

        public List<double> Calculate(params Mod[] mods)
        {
            mods = mods.Select(m => m.CreateCopy()).ToArray();

            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));

            var strains = difficultyCalculator.CalculateStrains(mods);
            double sectionLength = difficultyCalculator.SectionLength * clock.Rate;
            IBeatmap playableBeatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            if (!playableBeatmap.HitObjects.Any())
                return new List<double>();

            var hitObjects = playableBeatmap.HitObjects.OrderBy(h => h.StartTime).ToList();
            double firstSectionEnd = Math.Ceiling(playableBeatmap.HitObjects.First().StartTime / sectionLength) * sectionLength;
            int currentSectionEnd = 0;
            int count = 0;

            for(int i = 0; i < hitObjects.Count; i++)
            {
                var h = hitObjects[i];

                while (h.StartTime > firstSectionEnd + currentSectionEnd * sectionLength)
                {
                    if (count > 0)
                    {
                        //check if safe
                        if (strains[currentSectionEnd] == 0)
                            strains[currentSectionEnd] = strains[currentSectionEnd - 1];
                    }
                    else
                    {
                        if (hitObjects[i - 1] is IHasEndTime)
                        {
                            var s = hitObjects[i - 1] as IHasEndTime;
                            if (s.EndTime > firstSectionEnd + (currentSectionEnd - 1) * sectionLength)
                                if (strains[currentSectionEnd] == 0)
                                    strains[currentSectionEnd] = strains[currentSectionEnd - 1];
                        }
                        strains[currentSectionEnd] = 0;
                    }

                    count = 0;
                    currentSectionEnd++;
                }

                count++;
            }

            return strains;
        }
    }
}
