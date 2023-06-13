// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    internal class CatchScoreV1Processor
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

        public CatchScoreV1Processor(IBeatmap baseBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
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
                case TinyDroplet:
                    scoreIncrease = 10;
                    increaseCombo = false;
                    break;

                case Droplet:
                    scoreIncrease = 100;
                    break;

                case Fruit:
                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    increaseCombo = true;
                    break;

                case Banana:
                    scoreIncrease = 1100;
                    increaseCombo = false;
                    isBonus = true;
                    break;

                case JuiceStream:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested);
                    return;

                case BananaShower:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested);
                    return;
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
