// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
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
            //maybe expose it form DifficultyCalculator
            double firstSectionEnd = Math.Ceiling(playableBeatmap.HitObjects.First().StartTime / sectionLength) * sectionLength;
            int currentSection = 0;
            int count = 0;

            for (int i = 0; i < hitObjects.Count && currentSection < strains.Count; i++)
            {
                var h = hitObjects[i];

                while (h.StartTime > firstSectionEnd + currentSection * sectionLength)
                {
                    if (count > 0)
                    {
                        
                        if (strains[currentSection].StarRating == 0)
                        {
                            if (currentSection > 0)
                            {
                                strains[currentSection].StarRating = strains[currentSection - 1].StarRating;
                            }
                            else
                            {
                                strains[currentSection].StarRating = strains.Max(s => s.StarRating) / 2;
                            }
                        }
                            
                    }
                    else
                    {
                        if (hitObjects[i - 1] is IHasEndTime s)
                        {
                            if (s.EndTime > firstSectionEnd + (currentSection - 1) * sectionLength)
                            {
                                //maybe do this without if
                                if (strains[currentSection].StarRating == 0)
                                    strains[currentSection] = strains[currentSection - 1];
                            }
                            else
                                strains[currentSection].StarRating = 0;
                        }
                        else
                        {
                            strains[currentSection].StarRating = 0;
                        }
                    }

                    count = 0;
                    currentSection++;
                }

                count++;
            }

            var returnList = new List<double>();
            strains.ForEach(s => returnList.Add(s.StarRating));
            return returnList;
        }
    }
}
