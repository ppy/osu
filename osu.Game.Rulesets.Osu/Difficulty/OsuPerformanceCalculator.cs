// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public new OsuDifficultyAttributes Attributes => (OsuDifficultyAttributes)base.Attributes;

        /// <summary>
        /// Aim, tap and acc values are combined using power mean with this as the exponent.
        /// </summary>
        private const double total_value_exponent = 1.5;

        /// <summary>
        /// This exponent is used to convert throughput to aim pp and tap skill to tap pp.
        /// </summary>
        private const double skill_to_pp_exponent = 2.7;

        /// <summary>
        /// Miss count is continuous so the first 0.5 miss doesn't count when we penalize misses.
        /// </summary>
        private const double miss_count_leniency = 0.5;

        private Mod[] mods;

        private double accuracy;
        private int scoreMaxCombo;
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;

        private double greatWindow;

        private double effectiveMissCount;

        public OsuPerformanceCalculator(Ruleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            countGreat = Score.Statistics.GetOrDefault(HitResult.Great);
            countOk = Score.Statistics.GetOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetOrDefault(HitResult.Miss);

            greatWindow = 79.5 - 6 * Attributes.OverallDifficulty;

            double multiplier = 2.14; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            // guess the number of misses + slider breaks from combo
            double comboBasedMissCount;

            if (Attributes.SliderCount == 0)
            {
                if (scoreMaxCombo < Attributes.MaxCombo)
                    comboBasedMissCount = (double)Attributes.MaxCombo / scoreMaxCombo;
                else
                    comboBasedMissCount = 0;
            }
            else
            {
                double fullComboThreshold = Attributes.MaxCombo - 0.1 * Attributes.SliderCount;
                if (scoreMaxCombo < fullComboThreshold)
                    comboBasedMissCount = fullComboThreshold / scoreMaxCombo;
                else
                    comboBasedMissCount = Math.Pow((Attributes.MaxCombo - scoreMaxCombo) / (0.1 * Attributes.SliderCount), 3);
            }

            effectiveMissCount = Math.Max(countMiss, comboBasedMissCount);

            // Custom multipliers for NoFail and SpunOut.
            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= Math.Max(0.90, 1.0 - 0.02 * effectiveMissCount);

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 1.0 - Math.Pow((double)Attributes.SpinnerCount / totalHits, 0.85);

            double aimValue = computeAimValue();
            double tapValue = computeTapValue();
            double accuracyValue = computeAccuracyValue();

            double totalValue = PowerMean.Of(new[] { aimValue, tapValue, accuracyValue }, total_value_exponent) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Tap", tapValue);
                categoryRatings.Add("Accuracy", accuracyValue);
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", Attributes.MaxCombo);
            }

            return totalValue;
        }

        private double computeAimValue()
        {
            if (Attributes.TotalObjectCount <= 1)
                return 0;

            // Get player's throughput according to combo
            int comboTpCount = Attributes.ComboThroughputs.Length;
            var comboPercentages = Generate.LinearSpaced(comboTpCount, 1.0 / comboTpCount, 1);

            double scoreComboPercentage = ((double)scoreMaxCombo) / Attributes.MaxCombo;
            double comboTp = LinearSpline.InterpolateSorted(comboPercentages, Attributes.ComboThroughputs)
                                         .Interpolate(scoreComboPercentage);

            // Get player's throughput according to miss count
            double missTp = LinearSpline.InterpolateSorted(Attributes.MissCounts, Attributes.MissThroughputs)
                                        .Interpolate(effectiveMissCount);
            missTp = Math.Max(missTp, 0);

            // Combine combo based throughput and miss count based throughput
            double tp = PowerMean.Of(comboTp, missTp, 20);

            // Hidden mod
            if (mods.Any(h => h is OsuModHidden))
            {
                double hiddenFactor = Attributes.AimHiddenFactor;

                // the buff starts decreasing at AR9.75 and reaches 0 at AR10.75
                if (Attributes.ApproachRate > 10.75)
                    hiddenFactor = 1;
                else if (Attributes.ApproachRate > 9.75)
                    hiddenFactor = 1 + (1 - Math.Pow(Math.Sin((Attributes.ApproachRate - 9.75) * Math.PI / 2), 2)) * (hiddenFactor - 1);

                tp *= hiddenFactor;
            }

            // Account for cheesing
            double modifiedAcc = getModifiedAcc();
            double accOnCheeseNotes = 1 - (1 - modifiedAcc) * Math.Sqrt(totalHits / Attributes.CheeseNoteCount);

            // accOnCheeseNotes can be negative. The formula below ensures a positive acc while
            // preserving the value when accOnCheeseNotes is close to 1
            double accOnCheeseNotesPositive = Math.Exp(accOnCheeseNotes - 1);
            double urOnCheeseNotes = 10 * greatWindow / (Math.Sqrt(2) * SpecialFunctions.ErfInv(accOnCheeseNotesPositive));
            double cheeseLevel = SpecialFunctions.Logistic(((urOnCheeseNotes * Attributes.AimDifficulty) - 3200) / 2000);
            double cheeseFactor = LinearSpline.InterpolateSorted(Attributes.CheeseLevels, Attributes.CheeseFactors)
                                              .Interpolate(cheeseLevel);

            if (mods.Any(m => m is OsuModTouchDevice))
                tp = Math.Min(tp, 1.47 * Math.Pow(tp, 0.8));

            double aimValue = tpToPP(tp * cheeseFactor);

            // penalize misses
            aimValue *= Math.Pow(0.96, Math.Max(effectiveMissCount - miss_count_leniency, 0));

            // Buff long maps
            aimValue *= 1 + (SpecialFunctions.Logistic((totalHits - 2800) / 500.0) - SpecialFunctions.Logistic(-2800 / 500.0)) * 0.22;

            // Buff very high AR and low AR
            double approachRateFactor = 1.0;

            if (Attributes.ApproachRate > 10)
            {
                approachRateFactor += (0.05 + 0.35 * Math.Pow(Math.Sin(Math.PI * Math.Min(totalHits, 1250) / 2500), 1.7)) *
                                      Math.Pow(Attributes.ApproachRate - 10, 2);
            }
            else if (Attributes.ApproachRate < 8.0)
                approachRateFactor += 0.01 * (8.0 - Attributes.ApproachRate);

            aimValue *= approachRateFactor;

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply object-based bonus for flashlight.
                aimValue *= 1.0 + 0.35 * Math.Min(1.0, totalHits / 200.0) +
                            (totalHits > 200
                                ? 0.3 * Math.Min(1.0, (totalHits - 200) / 300.0) +
                                  (totalHits > 500 ? (totalHits - 500) / 2000.0 : 0.0)
                                : 0.0);
            }

            // Scale the aim value down with accuracy
            double accLeniency = greatWindow * Attributes.AimDifficulty / 300;
            double accPenalty = (0.09 / (accuracy - 1.3) + 0.3) * (accLeniency + 1.5);
            aimValue *= 0.2 + SpecialFunctions.Logistic(-((accPenalty - 0.24953) / 0.18));

            return aimValue;
        }

        private double computeTapValue()
        {
            if (Attributes.TotalObjectCount <= 1)
                return 0;

            double modifiedAcc = getModifiedAcc();

            // Assume SS for non-stream parts
            double accOnStreams = 1 - (1 - modifiedAcc) * Math.Sqrt(totalHits / Attributes.StreamNoteCount);

            // accOnStreams can be negative. The formula below ensures a positive acc while
            // preserving the value when accOnStreams is close to 1
            double accOnStreamsPositive = Math.Exp(accOnStreams - 1);

            double urOnStreams = 10 * greatWindow / (Math.Sqrt(2) * SpecialFunctions.ErfInv(accOnStreamsPositive));

            double mashLevel = SpecialFunctions.Logistic(((urOnStreams * Attributes.TapDifficulty) - 4000) / 1000);

            double tapSkill = mashLevel * Attributes.MashTapDifficulty + (1 - mashLevel) * Attributes.TapDifficulty;

            double tapValue = tapSkillToPP(tapSkill);

            // Buff very high acc on streams
            double accBuff = Math.Exp((accOnStreams - 1) * 60) * tapValue * 0.2;
            tapValue += accBuff;

            // Scale tap value down with accuracy
            double odScale = SpecialFunctions.Logistic(16.0 - greatWindow) * 0.04; // lenient curve for extreme OD
            double accFactor = 0.5 + 0.5 * (Math.Pow(SpecialFunctions.Logistic((accuracy - 0.9543 + 1.83 * odScale) / 0.025 + odScale), 0.2) + SpecialFunctions.Logistic(-3.5));
            tapValue *= accFactor;

            // Penalize misses and 50s exponentially
            tapValue *= Math.Pow(0.93, Math.Max(effectiveMissCount - miss_count_leniency, 0));
            tapValue *= Math.Pow(0.98, countMeh < totalHits / 500.0 ? 0.5 * countMeh : countMeh - totalHits / 500.0 * 0.5);

            // Buff very high AR
            double approachRateFactor = 1.0;
            double ar11LengthBuff = 0.8 * (SpecialFunctions.Logistic(totalHits / 500) - 0.5);
            if (Attributes.ApproachRate > 10.33)
                approachRateFactor += ar11LengthBuff * (Attributes.ApproachRate - 10.33) / 0.67;

            tapValue *= approachRateFactor;

            return tapValue;
        }

        private double computeAccuracyValue()
        {
            double fingerControlDiff = Attributes.FingerControlDifficulty;

            double modifiedAcc = getModifiedAcc();

            // technically accOnCircles = modifiedAcc
            // -0.003 exists so that the difference between 99.5% and 100% is not too big
            double accOnCircles = modifiedAcc - 0.003;

            // accOnCircles can be negative. The formula below ensures a positive acc while
            // preserving the value when accOnCircles is close to 1
            double accOnCirclesPositive = Math.Exp(accOnCircles - 1);

            // add 20 to greatWindow to nerf high OD
            double deviationOnCircles = (greatWindow + 20) / (Math.Sqrt(2) * SpecialFunctions.ErfInv(accOnCirclesPositive));
            double accuracyValue = Math.Pow(deviationOnCircles, -2.2) * Math.Pow(fingerControlDiff, 0.5) * 46000;

            // scale acc pp with misses
            accuracyValue *= Math.Pow(0.96, Math.Max(effectiveMissCount - miss_count_leniency, 0));

            // nerf short maps
            double lengthFactor = Attributes.Length < 120 ? SpecialFunctions.Logistic((Attributes.Length - 300) / 60.0) + SpecialFunctions.Logistic(2.5) - SpecialFunctions.Logistic(-2.5) : SpecialFunctions.Logistic(Attributes.Length / 60.0);
            accuracyValue *= lengthFactor;

            if (mods.Any(m => m is OsuModHidden))
                accuracyValue *= 1.08;
            if (mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02;

            return accuracyValue;
        }

        private double getModifiedAcc()
        {
            // Treat 300 as 300, 100 as 200, 50 as 100
            // Assume all 300s on sliders/spinners and exclude them from the calculation. In other words we're
            // estimating the scorev2 acc from scorev1 acc.
            // Add 2 to countHitCircles in the denominator so that later erfinv gives resonable result for ss scores
            double modifiedAcc = ((countGreat - (totalHits - Attributes.HitCircleCount)) * 3 + countOk * 2 + countMeh) /
                                 ((Attributes.HitCircleCount + 2) * 3);
            return modifiedAcc;
        }

        private double tpToPP(double tp) => Math.Pow(tp, skill_to_pp_exponent) * 0.118;

        private double tapSkillToPP(double tapSkill) => Math.Pow(tapSkill, skill_to_pp_exponent) * 0.115;

        private double fingerControlDiffToPP(double fingerControlDiff) => Math.Pow(fingerControlDiff, skill_to_pp_exponent);

        private double totalHits => countGreat + countOk + countMeh + countMiss;
        private double totalSuccessfulHits => countGreat + countOk + countMeh;
    }
}
