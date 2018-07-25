// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Collections.Generic;
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

        public List<double> OsuDifficultySectionRating  = new List<double>();

        public List<double> OsuDifficultyAt (IBeatmap beatmap, double timeRate)
        {
            //throws list of difficulties(strains) at sections (or at hitobjects)
            //remember that first section is double-timed

            //if (!beatmap.HitObjects.Any())
            //    return new List<double>();

            //private OsuDifficultyBeatmap difficultyBeatmap = new OsuDifficultyBeatmap(beatmap.HitObjects.Cast<OsuHitObject>().ToList(), timeRate);
            //Skill[] skills =
            //{
            //    new Aim(),
            //    new Speed()
            //};

            //double sectionLength = section_length * timeRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            //double currentSectionEnd = 2 * sectionLength;

            //foreach ( OsuDifficultyHitObject h in difficultyBeatmap )
            //{
            //    while (h.BaseObject.StartTime > currentSectionEnd)
            //    {
            //        foreach (Skill s in skills)
            //        {
            //            s.SaveCurrentPeak()
            //            s.StartNewSectionFrom(currentSectionEnd);
            //        }

            //        currentSectionEnd += sectionLength;
            //    }

            //    foreach (Skill s in skills)
            //        s.Process(h);
            //}

            //private List<double> aimRating = new List<double>();
            //private List<double> speedRating = new List<double>();
            //private List<double> osuDifficultySectionRating = new List<double>();

            //foreach(double x in Skills[0].StrainPeaks)
            //{
            //    aimRating.Add(x * difficulty_multiplier);
            //};

            //foreach(double x in Skills[1].StrainPeaks)
            //{
            //    speedRating.Add(x * difficulty_multiplier);
            //}

            //for (int x = 0; x < aimRating.Count(); x++)
            //{
            //    osuDifficultySectionRating.Add(aimRating[x] + speedRating[x] + Math.Abs(aimRating[x] - speedRating[x]) / 2);
            //}

            //return osuDifficultySectionRating;
        }

        //Copy some code from here
        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double timeRate)
        {
            if (!beatmap.HitObjects.Any())
                return new OsuDifficultyAttributes(mods, 0);

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

            // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            double hitWindowGreat = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / timeRate;
            double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / timeRate;

            int maxCombo = beatmap.HitObjects.Count();
            // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
            maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

            return new OsuDifficultyAttributes(mods, starRating)
            {
                AimStrain = aimRating,
                SpeedStrain = speedRating,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                MaxCombo = maxCombo
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
