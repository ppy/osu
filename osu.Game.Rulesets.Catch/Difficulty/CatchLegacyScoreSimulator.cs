// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    internal class CatchLegacyScoreSimulator : ILegacyScoreSimulator
    {
        public int AccuracyScore { get; private set; }

        public int ComboScore { get; private set; }

        public double BonusScoreRatio => legacyBonusScore == 0 ? 0 : (double)modernBonusScore / legacyBonusScore;

        private int legacyBonusScore;
        private int modernBonusScore;
        private int combo;

        private double scoreMultiplier;

        public void Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
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
                    bonusResult = HitResult.LargeBonus;
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
