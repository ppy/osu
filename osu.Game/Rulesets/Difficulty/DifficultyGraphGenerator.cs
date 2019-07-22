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

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public class DifficultyGraphGenerator
    {
        private readonly Ruleset ruleset;
        private readonly WorkingBeatmap beatmap;
        private readonly DifficultyCalculator difficultyCalculator;

        protected DifficultyGraphGenerator(Ruleset ruleset, WorkingBeatmap beatmap, DifficultyCalculator difficultyCalculator)
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
            double currentSectionEnd = firstSectionEnd;
            int count = 0;

            foreach(HitObject h in hitObjects)
            {
                while (h.StartTime > currentSectionEnd)
                {
                    if (count > 0)
                    {
                        if (strains[] = 0)
                            strains[] = strains[-1];
                    }
                    else
                        strains[] = 0;

                    currentSectionEnd += sectionLength;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }
        }
    }
}
