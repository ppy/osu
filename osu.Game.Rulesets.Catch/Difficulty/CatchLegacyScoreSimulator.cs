// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    internal class CatchLegacyScoreSimulator : ILegacyScoreSimulator
    {
        private readonly ScoreProcessor scoreProcessor = new CatchScoreProcessor();

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

            scoreMultiplier = LegacyRulesetExtensions.CalculateDifficultyPeppyStars(baseBeatmap.Difficulty, objectCount, drainLength);

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
                        simulateHit(nested, ref attributes);
                    return;

                case BananaShower:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested, ref attributes);
                    return;
            }

            if (addScoreComboMultiplier)
            {
                // ReSharper disable once PossibleLossOfFraction (intentional to match osu-stable...)
                attributes.ComboScore += (int)(Math.Max(0, combo - 1) * (scoreIncrease / 25 * scoreMultiplier));
            }

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
                    case CatchModNoFail:
                        multiplier *= scoreV2 ? 1.0 : 0.5;
                        break;

                    case CatchModEasy:
                        multiplier *= 0.5;
                        break;

                    case CatchModHalfTime:
                    case CatchModDaycore:
                        multiplier *= 0.3;
                        break;

                    case CatchModHidden:
                        multiplier *= scoreV2 ? 1.0 : 1.06;
                        break;

                    case CatchModHardRock:
                        multiplier *= 1.12;
                        break;

                    case CatchModDoubleTime:
                    case CatchModNightcore:
                        multiplier *= 1.06;
                        break;

                    case CatchModFlashlight:
                        multiplier *= 1.12;
                        break;

                    case CatchModRelax:
                        return 0;
                }
            }

            return multiplier;
        }
    }
}
