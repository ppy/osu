// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        public OsuDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double timeRate)
        {
            OsuDifficultyBeatmap difficultyBeatmap = new OsuDifficultyBeatmap(beatmap.HitObjects.Cast<OsuHitObject>().ToList(), timeRate);
            Skill[] skills =
            {
                new Aim(),
                new Speed()
            };

            double sectionLength = section_length * timeRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            double currentSectionEnd = 2 * sectionLength;

            foreach (OsuDifficultyHitObject h in difficultyBeatmap)
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

            return new OsuDifficultyAttributes(mods, starRating)
            {
                AimStrain = aimRating,
                SpeedStrain = speedRating
            };
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
