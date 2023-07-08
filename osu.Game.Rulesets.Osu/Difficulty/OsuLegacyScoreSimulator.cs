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
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    internal class OsuLegacyScoreSimulator : ILegacyScoreSimulator
    {
        public int AccuracyScore { get; private set; }

        public int ComboScore { get; private set; }

        public double BonusScoreRatio => legacyBonusScore == 0 ? 0 : (double)modernBonusScore / legacyBonusScore;

        private int legacyBonusScore;
        private int modernBonusScore;
        private int combo;

        private double scoreMultiplier;
        private IBeatmap playableBeatmap = null!;

        public void Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
            this.playableBeatmap = playableBeatmap;

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

            scoreMultiplier = difficultyPeppyStars * mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);

            foreach (var obj in playableBeatmap.HitObjects)
                simulateHit(obj);
        }

        private void simulateHit(HitObject hitObject)
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
                        simulateHit(nested);

                    scoreIncrease = 300;
                    increaseCombo = false;
                    addScoreComboMultiplier = true;
                    break;

                case Spinner spinner:
                    // The spinner object applies a lenience because gameplay mechanics differ from osu-stable.
                    // We'll redo the calculations to match osu-stable here...
                    const double maximum_rotations_per_second = 477.0 / 60;
                    double minimumRotationsPerSecond = IBeatmapDifficultyInfo.DifficultyRange(playableBeatmap.Difficulty.OverallDifficulty, 3, 5, 7.5);
                    double secondsDuration = spinner.Duration / 1000;

                    // The total amount of half spins possible for the entire spinner.
                    int totalHalfSpinsPossible = (int)(secondsDuration * maximum_rotations_per_second * 2);
                    // The amount of half spins that are required to successfully complete the spinner (i.e. get a 300).
                    int halfSpinsRequiredForCompletion = (int)(secondsDuration * minimumRotationsPerSecond);
                    // To be able to receive bonus points, the spinner must be rotated another 1.5 times.
                    int halfSpinsRequiredBeforeBonus = halfSpinsRequiredForCompletion + 3;

                    for (int i = 0; i <= totalHalfSpinsPossible; i++)
                    {
                        if (i > halfSpinsRequiredBeforeBonus && (i - halfSpinsRequiredBeforeBonus) % 2 == 0)
                            simulateHit(new SpinnerBonusTick());
                        else if (i > 1 && i % 2 == 0)
                            simulateHit(new SpinnerTick());
                    }

                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;
            }

            if (addScoreComboMultiplier)
            {
                // ReSharper disable once PossibleLossOfFraction (intentional to match osu-stable...)
                ComboScore += (int)(Math.Max(0, combo - 1) * (scoreIncrease / 25 * scoreMultiplier));
            }

            if (isBonus)
            {
                legacyBonusScore += scoreIncrease;
                modernBonusScore += Judgement.ToNumericResult(bonusResult);
            }
            else
                AccuracyScore += scoreIncrease;

            if (increaseCombo)
                combo++;
        }
    }
}
