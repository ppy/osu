// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Difficulty.MathUtil;


namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double aimMultiplier = 0.585;
        private const double tapMultiplier = 0.732;
        private const double srExponent = 0.85;

        public OsuDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var hitObjectsNoSpinner = beatmap.HitObjects.Where(obj => !(obj is Spinner))
                                                        .Select(obj => (OsuHitObject)obj).ToList();
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods};

            double mapLength = (beatmap.HitObjects.Last().StartTime - beatmap.HitObjects.First().StartTime) / 1000;


            // Tap
            (var tapDiff, var streamNoteCount, var mashLevels, var tapSkills, var strainHistory) =
                Tap.CalculateTapAttributes(hitObjectsNoSpinner, clockRate);

            // Aim
            (var aimDiff, var fcTimeTP, var missTPs, var missCounts, var cheeseNoteCount, var cheeseLevels, var cheeseFactors) =
                Aim.CalculateAimAttributes(hitObjectsNoSpinner, clockRate, strainHistory);

            double tapSR = tapMultiplier * Math.Pow(tapDiff, srExponent);
            double aimSR = aimMultiplier * Math.Pow(aimDiff, srExponent);
            double sr = Mean.PowerMean(tapSR, aimSR, 7) * 1.069;

            // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            double hitWindowGreat = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / clockRate;
            double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            int maxCombo = beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
            maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

            return new OsuDifficultyAttributes
            {
                StarRating = sr,
                Mods = mods,

                TapSR = tapSR,
                TapDiff = tapDiff,
                StreamNoteCount = streamNoteCount,
                MashLevels = mashLevels,
                TapSkills = tapSkills,

                AimSR = aimSR,
                AimDiff = aimDiff,
                MissTPs = missTPs,
                MissCounts = missCounts,
                CheeseNoteCount = cheeseNoteCount,
                CheeseLevels = cheeseLevels,
                CheeseFactors = cheeseFactors,

                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                MaxCombo = maxCombo
            };
        }


        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new Aim(),
            new Speed()
        };

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            throw new NotImplementedException();
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            throw new NotImplementedException();
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
