// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    internal class TaikoLegacyScoreSimulator : ILegacyScoreSimulator
    {
        private readonly ScoreProcessor scoreProcessor = new TaikoScoreProcessor();

        private int legacyBonusScore;
        private int standardisedBonusScore;
        private int combo;

        private int difficultyPeppyStars;
        private IBeatmap playableBeatmap = null!;

        public LegacyScoreAttributes Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap)
        {
            this.playableBeatmap = playableBeatmap;

            IBeatmap baseBeatmap = workingBeatmap.Beatmap;

            int countNormal = 0;
            int countSlider = 0;
            int countSpinner = 0;

            foreach (HitObject obj in baseBeatmap.HitObjects)
            {
                switch (obj)
                {
                    case IHasPath:
                        countSlider++;
                        break;

                    case IHasDuration:
                        countSpinner++;
                        break;

                    default:
                        countNormal++;
                        break;
                }
            }

            int objectCount = countNormal + countSlider + countSpinner;

            int drainLength = 0;

            if (baseBeatmap.HitObjects.Count > 0)
            {
                int breakLength = baseBeatmap.Breaks.Select(b => (int)Math.Round(b.EndTime) - (int)Math.Round(b.StartTime)).Sum();
                drainLength = ((int)Math.Round(baseBeatmap.HitObjects[^1].StartTime) - (int)Math.Round(baseBeatmap.HitObjects[0].StartTime) - breakLength) / 1000;
            }

            difficultyPeppyStars = (int)Math.Round(
                (baseBeatmap.Difficulty.DrainRate
                 + baseBeatmap.Difficulty.OverallDifficulty
                 + baseBeatmap.Difficulty.CircleSize
                 + Math.Clamp((float)objectCount / drainLength * 8, 0, 16)) / 38 * 5);

            LegacyScoreAttributes attributes = new LegacyScoreAttributes();

            foreach (var obj in playableBeatmap.HitObjects)
                simulateHit(obj, ref attributes);

            attributes.BonusScoreRatio = legacyBonusScore == 0 ? 0 : (double)standardisedBonusScore / legacyBonusScore;
            attributes.BonusScore = legacyBonusScore;
            attributes.MaxCombo = combo;

            return attributes;
        }

        private void simulateHit(HitObject hitObject, ref LegacyScoreAttributes attributes)
        {
            bool increaseCombo = true;
            bool addScoreComboMultiplier = false;

            bool isBonus = false;
            HitResult bonusResult = HitResult.None;

            int scoreIncrease = 0;

            switch (hitObject)
            {
                case SwellTick:
                    scoreIncrease = 300;
                    increaseCombo = false;
                    break;

                case DrumRollTick:
                    scoreIncrease = 300;
                    increaseCombo = false;
                    isBonus = true;
                    bonusResult = HitResult.SmallBonus;
                    break;

                case Swell swell:
                    // The taiko swell generally does not match the osu-stable implementation in any way.
                    // We'll redo the calculations to match osu-stable here...

                    // Normally, this value depends on the final overall difficulty. For simplicity, we'll only consider the worst case that maximises rotations.
                    const double minimum_rotations_per_second = 7.5;

                    // The amount of half spins that are required to successfully complete the spinner (i.e. get a 300).
                    int halfSpinsRequiredForCompletion = (int)(swell.Duration / 1000 * minimum_rotations_per_second);
                    halfSpinsRequiredForCompletion = (int)Math.Max(1, halfSpinsRequiredForCompletion * 1.65f);

                    //
                    // Normally, this multiplier depends on the active mods (DT = 0.75, HT = 1.5). For simplicity, we'll only consider the worst case that maximises rotations.
                    // This way, scores remain beatable at the cost of the conversion being slightly inaccurate.
                    //   - A perfect DT/NM score will have less than 1M total score (excluding bonus).
                    //   - A perfect HT score will have 1M total score (excluding bonus).
                    //
                    halfSpinsRequiredForCompletion = Math.Max(1, (int)(halfSpinsRequiredForCompletion * 1.5f));

                    for (int i = 0; i <= halfSpinsRequiredForCompletion; i++)
                        simulateHit(new SwellTick(), ref attributes);

                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    increaseCombo = false;
                    isBonus = true;
                    bonusResult = HitResult.LargeBonus;
                    break;

                case Hit:
                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;

                case DrumRoll:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested, ref attributes);
                    return;
            }

            if (hitObject is DrumRollTick tick)
            {
                if (playableBeatmap.ControlPointInfo.EffectPointAt(tick.Parent.StartTime).KiaiMode)
                    scoreIncrease = (int)(scoreIncrease * 1.2f);

                if (tick.IsStrong)
                    scoreIncrease += scoreIncrease / 5;
            }

            // The score increase directly contributed to by the combo-multiplied portion.
            int comboScoreIncrease = 0;

            if (addScoreComboMultiplier)
            {
                int oldScoreIncrease = scoreIncrease;

                scoreIncrease += scoreIncrease / 35 * 2 * (difficultyPeppyStars + 1) * (Math.Min(100, combo) / 10);

                if (hitObject is Swell)
                {
                    if (playableBeatmap.ControlPointInfo.EffectPointAt(hitObject.GetEndTime()).KiaiMode)
                        scoreIncrease = (int)(scoreIncrease * 1.2f);
                }
                else
                {
                    if (playableBeatmap.ControlPointInfo.EffectPointAt(hitObject.StartTime).KiaiMode)
                        scoreIncrease = (int)(scoreIncrease * 1.2f);
                }

                comboScoreIncrease = scoreIncrease - oldScoreIncrease;
            }

            if (hitObject is Swell || (hitObject is TaikoStrongableHitObject strongable && strongable.IsStrong))
            {
                scoreIncrease *= 2;
                comboScoreIncrease *= 2;
            }

            scoreIncrease -= comboScoreIncrease;

            if (addScoreComboMultiplier)
                attributes.ComboScore += comboScoreIncrease;

            if (isBonus)
            {
                legacyBonusScore += scoreIncrease;
                standardisedBonusScore += scoreProcessor.GetBaseScoreForResult(bonusResult);
            }
            else
                attributes.AccuracyScore += scoreIncrease;

            if (increaseCombo)
                combo++;
        }

        public double GetLegacyScoreMultiplier(IReadOnlyList<Mod> mods, LegacyBeatmapConversionDifficultyInfo difficulty)
        {
            bool scoreV2 = mods.Any(m => m is ModScoreV2);

            double multiplier = 1.0;

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case TaikoModNoFail:
                        multiplier *= scoreV2 ? 1.0 : 0.5;
                        break;

                    case TaikoModEasy:
                        multiplier *= 0.5;
                        break;

                    case TaikoModHalfTime:
                    case TaikoModDaycore:
                        multiplier *= 0.3;
                        break;

                    case TaikoModHidden:
                    case TaikoModHardRock:
                        multiplier *= 1.06;
                        break;

                    case TaikoModDoubleTime:
                    case TaikoModNightcore:
                    case TaikoModFlashlight:
                        multiplier *= 1.12;
                        break;

                    case TaikoModRelax:
                        return 0;
                }
            }

            return multiplier;
        }
    }
}
