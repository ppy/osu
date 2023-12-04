// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    internal class OsuLegacyScoreSimulator : ILegacyScoreSimulator
    {
        private int legacyBonusScore;
        private int standardisedBonusScore;
        private int combo;

        private double scoreMultiplier;

        public LegacyScoreAttributes Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap)
        {
            IBeatmap baseBeatmap = workingBeatmap.Beatmap;

            int countNormal = 0;
            int countSlider = 0;
            int countSpinner = 0;

            foreach (HitObject obj in workingBeatmap.Beatmap.HitObjects)
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

            int difficultyPeppyStars = (int)Math.Round(
                (baseBeatmap.Difficulty.DrainRate
                 + baseBeatmap.Difficulty.OverallDifficulty
                 + baseBeatmap.Difficulty.CircleSize
                 + Math.Clamp((float)objectCount / drainLength * 8, 0, 16)) / 38 * 5);

            scoreMultiplier = difficultyPeppyStars;

            LegacyScoreAttributes attributes = new LegacyScoreAttributes();

            foreach (var obj in playableBeatmap.HitObjects)
                simulateHit(obj, ref attributes);

            attributes.BonusScoreRatio = legacyBonusScore == 0 ? 0 : (double)standardisedBonusScore / legacyBonusScore;

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
                case SliderHeadCircle:
                case SliderTailCircle:
                case SliderRepeat:
                    scoreIncrease = 30;
                    break;

                case SliderTick:
                    scoreIncrease = 10;
                    break;

                case SpinnerBonusTick:
                    scoreIncrease = 1100;
                    increaseCombo = false;
                    isBonus = true;
                    bonusResult = HitResult.LargeBonus;
                    break;

                case SpinnerTick:
                    scoreIncrease = 100;
                    increaseCombo = false;
                    isBonus = true;
                    bonusResult = HitResult.SmallBonus;
                    break;

                case HitCircle:
                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;

                case Slider:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested, ref attributes);

                    scoreIncrease = 300;
                    increaseCombo = false;
                    addScoreComboMultiplier = true;
                    break;

                case Spinner spinner:
                    // The spinner object applies a lenience because gameplay mechanics differ from osu-stable.
                    // We'll redo the calculations to match osu-stable here...
                    const double maximum_rotations_per_second = 477.0 / 60;

                    // Normally, this value depends on the final overall difficulty. For simplicity, we'll only consider the worst case that maximises bonus score.
                    // As we're primarily concerned with computing the maximum theoretical final score,
                    // this will have the final effect of slightly underestimating bonus score achieved on stable when converting from score V1.
                    const double minimum_rotations_per_second = 3;

                    double secondsDuration = spinner.Duration / 1000;

                    // The total amount of half spins possible for the entire spinner.
                    int totalHalfSpinsPossible = (int)(secondsDuration * maximum_rotations_per_second * 2);
                    // The amount of half spins that are required to successfully complete the spinner (i.e. get a 300).
                    int halfSpinsRequiredForCompletion = (int)(secondsDuration * minimum_rotations_per_second);
                    // To be able to receive bonus points, the spinner must be rotated another 1.5 times.
                    int halfSpinsRequiredBeforeBonus = halfSpinsRequiredForCompletion + 3;

                    for (int i = 0; i <= totalHalfSpinsPossible; i++)
                    {
                        if (i > halfSpinsRequiredBeforeBonus && (i - halfSpinsRequiredBeforeBonus) % 2 == 0)
                            simulateHit(new SpinnerBonusTick(), ref attributes);
                        else if (i > 1 && i % 2 == 0)
                            simulateHit(new SpinnerTick(), ref attributes);
                    }

                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;
            }

            if (addScoreComboMultiplier)
            {
                // ReSharper disable once PossibleLossOfFraction (intentional to match osu-stable...)
                attributes.ComboScore += (int)(Math.Max(0, combo - 1) * (scoreIncrease / 25 * scoreMultiplier));
            }

            if (isBonus)
            {
                legacyBonusScore += scoreIncrease;
                standardisedBonusScore += Judgement.ToNumericResult(bonusResult);
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
                    case OsuModNoFail:
                        multiplier *= scoreV2 ? 1.0 : 0.5;
                        break;

                    case OsuModEasy:
                        multiplier *= 0.5;
                        break;

                    case OsuModHalfTime:
                    case OsuModDaycore:
                        multiplier *= 0.3;
                        break;

                    case OsuModHidden:
                        multiplier *= 1.06;
                        break;

                    case OsuModHardRock:
                        multiplier *= scoreV2 ? 1.10 : 1.06;
                        break;

                    case OsuModDoubleTime:
                    case OsuModNightcore:
                        multiplier *= scoreV2 ? 1.20 : 1.12;
                        break;

                    case OsuModFlashlight:
                        multiplier *= 1.12;
                        break;

                    case OsuModSpunOut:
                        multiplier *= 0.9;
                        break;

                    case OsuModRelax:
                    case OsuModAutopilot:
                        return 0;
                }
            }

            return multiplier;
        }
    }
}
