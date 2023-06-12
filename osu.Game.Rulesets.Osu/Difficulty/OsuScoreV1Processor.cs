// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    internal class OsuScoreV1Processor
    {
        public int TotalScore => BaseScore + ComboScore + BonusScore;

        /// <summary>
        /// Amount of score that is combo-and-difficulty-multiplied, excluding mod multipliers.
        /// </summary>
        public int ComboScore { get; private set; }

        /// <summary>
        /// Amount of score that is NOT combo-and-difficulty-multiplied.
        /// </summary>
        public int BaseScore { get; private set; }

        /// <summary>
        /// Amount of score whose judgements would be treated as "bonus" in ScoreV2.
        /// </summary>
        public int BonusScore { get; private set; }

        private int combo;

        private readonly double scoreMultiplier;
        private readonly IBeatmap playableBeatmap;

        public OsuScoreV1Processor(IBeatmap baseBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
            this.playableBeatmap = playableBeatmap;

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

            int difficultyPeppyStars = (int)Math.Round(
                (baseBeatmap.Difficulty.DrainRate
                 + baseBeatmap.Difficulty.OverallDifficulty
                 + baseBeatmap.Difficulty.CircleSize
                 + Math.Clamp(objectCount / baseBeatmap.Difficulty.DrainRate * 8, 0, 16)) / 38 * 5);

            scoreMultiplier = difficultyPeppyStars * mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);

            foreach (var obj in playableBeatmap.HitObjects)
                simulateHit(obj);
        }

        private void simulateHit(HitObject hitObject)
        {
            bool increaseCombo = true;
            bool addScoreComboMultiplier = false;
            bool isBonus = false;

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
                    break;

                case SpinnerTick:
                    scoreIncrease = 100;
                    increaseCombo = false;
                    isBonus = true;
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
                BonusScore += scoreIncrease;
            else
                BaseScore += scoreIncrease;

            if (increaseCombo)
                combo++;
        }
    }
}
