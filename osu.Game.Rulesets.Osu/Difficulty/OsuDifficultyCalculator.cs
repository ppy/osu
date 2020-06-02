// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double aim_multiplier = 0.641;
        private const double tap_multiplier = 0.641;
        private const double finger_control_multiplier = 1.245;

        private const double sr_exponent = 0.83;

        public OsuDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var hitObjects = beatmap.HitObjects as List<OsuHitObject>;

            double mapLength = 0;
            if (beatmap.HitObjects.Count > 0)
                mapLength = (beatmap.HitObjects.Last().StartTime - beatmap.HitObjects.First().StartTime) / 1000 / clockRate;

            double preemptNoClockRate = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450);
            var noteDensities = NoteDensity.CalculateNoteDensities(hitObjects, preemptNoClockRate);

            // Tap
            (var tapDiff, var streamNoteCount, var mashTapDiff, var strainHistory) =
                Tap.CalculateTapAttributes(hitObjects, clockRate);

            // Finger Control
            double fingerControlDiff = FingerControl.CalculateFingerControlDiff(hitObjects, clockRate);

            // Aim
            (var aimDiff, var aimHiddenFactor, var comboTps, var missTps, var missCounts,
                    var cheeseNoteCount, var cheeseLevels, var cheeseFactors, var graphText) =
                Aim.CalculateAimAttributes(hitObjects, clockRate, strainHistory, noteDensities);

            // graph for aim
            string graphFilePath = Path.Combine("cache", $"graph_{beatmap.BeatmapInfo.OnlineBeatmapID}_{string.Join(string.Empty, mods.Select(x => x.Acronym))}.txt");
            File.WriteAllText(graphFilePath, graphText);

            double tapSr = tap_multiplier * Math.Pow(tapDiff, sr_exponent);
            double aimSr = aim_multiplier * Math.Pow(aimDiff, sr_exponent);
            double fingerControlSr = finger_control_multiplier * Math.Pow(fingerControlDiff, sr_exponent);
            double sr = Mean.PowerMean(new[] { tapSr, aimSr, fingerControlSr }, 7) * 1.131;

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            double hitWindowGreat = (int)(hitWindows.WindowFor(HitResult.Great)) / clockRate;
            double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            int maxCombo = beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
            maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

            return new OsuDifficultyAttributes
            {
                StarRating = sr,
                Mods = mods,
                Length = mapLength,

                TapSr = tapSr,
                TapDiff = tapDiff,
                StreamNoteCount = streamNoteCount,
                MashTapDiff = mashTapDiff,

                FingerControlSr = fingerControlSr,
                FingerControlDiff = fingerControlDiff,

                AimSr = aimSr,
                AimDiff = aimDiff,
                AimHiddenFactor = aimHiddenFactor,
                ComboTps = comboTps,
                MissTps = missTps,
                MissCounts = missCounts,
                CheeseNoteCount = cheeseNoteCount,
                CheeseLevels = cheeseLevels,
                CheeseFactors = cheeseFactors,

                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                MaxCombo = maxCombo
            };
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap)
        {
            throw new NotImplementedException();
        }

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
