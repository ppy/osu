// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const int section_length = 400;
        private const double difficulty_multiplier = 0.0675;

        public OsuDifficultyCalculator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public OsuDifficultyCalculator(IBeatmap beatmap, Mod[] mods)
            : base(beatmap, mods)
        {
        }

        public override double Calculate(Dictionary<string, object> categoryDifficulty = null)
        {
            OsuDifficultyBeatmap beatmap = new OsuDifficultyBeatmap((List<OsuHitObject>)Beatmap.HitObjects, TimeRate);
            Skill[] skills =
            {
                new Aim(),
                new Speed()
            };

            double sectionLength = section_length * TimeRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            double currentSectionEnd = 2 * sectionLength;

            foreach (OsuDifficultyHitObject h in beatmap)
            {
                while (h.BaseObject.StartTime > currentSectionEnd)
                {
                    foreach (Skill s in skills)
                    {
                        s.SaveCurrentPeak();
                        s.StartNewSectionFrom(currentSectionEnd);
                    }

                    currentSectionEnd += sectionLength;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;

            double starRating = aimRating + speedRating + Math.Abs(aimRating - speedRating) / 2;

            if (categoryDifficulty != null)
            {
                // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be remoevd in the future
                double hitWindowGreat = (int)(Beatmap.HitObjects.First().HitWindows.Great / 2) / TimeRate;
                double preEmpt = (int)BeatmapDifficulty.DifficultyRange(Beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / TimeRate;

                int maxCombo = Beatmap.HitObjects.Count();
                // Add the ticks + tail of the slider. 1 is subtracted because the "headcircle" would be counted twice (once for the slider itself in the line above)
                maxCombo += Beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

                categoryDifficulty["Aim"] = aimRating;
                categoryDifficulty["Speed"] = speedRating;
                categoryDifficulty["AR"] = preEmpt > 1200 ? (1800 - preEmpt) / 120 : (1200 - preEmpt) / 150 + 5;
                categoryDifficulty["OD"] = (80 - hitWindowGreat) / 6;
                categoryDifficulty["Max combo"] = maxCombo;
            }

            return starRating;
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
        };
    }
}
