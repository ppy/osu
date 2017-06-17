﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.OsuDifficulty.Preprocessing;
using osu.Game.Rulesets.Osu.OsuDifficulty.Skills;

namespace osu.Game.Rulesets.Osu.OsuDifficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator<OsuHitObject>
    {
        private const int section_length = 400;
        private const double difficulty_multiplier = 0.0675;

        public OsuDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        protected override void PreprocessHitObjects()
        {
            foreach (OsuHitObject h in Objects)
                (h as Slider)?.Curve?.Calculate();
        }

        protected override double CalculateInternal(Dictionary<string, string> categoryDifficulty)
        {
            OsuDifficultyBeatmap beatmap = new OsuDifficultyBeatmap(Objects);
            Skill[] skills =
            {
                new Aim(),
                new Speed()
            };

            double sectionEnd = section_length / TimeRate;
            foreach (OsuDifficultyHitObject h in beatmap)
            {
                while (h.BaseObject.StartTime > sectionEnd)
                {
                    foreach (Skill s in skills)
                    {
                        s.SaveCurrentPeak();
                        s.StartNewSectionFrom(sectionEnd);
                    }

                    sectionEnd += section_length;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;

            double starRating = aimRating + speedRating + Math.Abs(aimRating - speedRating) / 2;

            if (categoryDifficulty != null)
            {
                categoryDifficulty.Add("Aim", aimRating.ToString("0.00"));
                categoryDifficulty.Add("Speed", speedRating.ToString("0.00"));
            }

            return starRating;
        }

        protected override BeatmapConverter<OsuHitObject> CreateBeatmapConverter() => new OsuBeatmapConverter();
    }
}
