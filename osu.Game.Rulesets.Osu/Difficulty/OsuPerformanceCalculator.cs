// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Rulesets.Osu.Difficulty.MathUtil;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public new OsuDifficultyAttributes Attributes => (OsuDifficultyAttributes)base.Attributes;

        private const double totalValueExponent = 1.5;
        private const double comboWeight = 0.5;
        private const double skillToPPExponent = 2.7;

        private readonly int countHitCircles;
        private readonly int countSliders;
        private readonly int beatmapMaxCombo;

        private Mod[] mods;

        private double accuracy;
        private int scoreMaxCombo;
        private int countGreat;
        private int countGood;
        private int countMeh;
        private int countMiss;

        private double greatWindow;

        public OsuPerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, ScoreInfo score)
            : base(ruleset, beatmap, score)
        {
            countHitCircles = Beatmap.HitObjects.Count(h => h is HitCircle);
            countSliders = Beatmap.HitObjects.Count(h => h is Slider);

            beatmapMaxCombo = Beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the "headcircle" would be counted twice (once for the slider itself in the line above)
            beatmapMaxCombo += Beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            countGreat = Convert.ToInt32(Score.Statistics[HitResult.Great]);
            countGood = Convert.ToInt32(Score.Statistics[HitResult.Good]);
            countMeh = Convert.ToInt32(Score.Statistics[HitResult.Meh]);
            countMiss = Convert.ToInt32(Score.Statistics[HitResult.Miss]);

            greatWindow = 79.5 - 6 * Attributes.OverallDifficulty;

            // Don't count scores made with supposedly unranked mods
            if (mods.Any(m => !m.Ranked))
                return 0;

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 2.14; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= 0.90;

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 0.95;

            double aimValue = computeAimValue();
            double tapValue = computeTapValue();
            double accuracyValue = computeAccuracyValue();

            double totalValue = Mean.PowerMean(new double[] { aimValue, tapValue, accuracyValue }, totalValueExponent) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Tap", tapValue);
                categoryRatings.Add("Accuracy", accuracyValue);
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", beatmapMaxCombo);
            }

            return totalValue;
        }

        private double computeAimValue()
        {

            // Guess the number of misaims from combo
            int effectiveMissCount = Math.Max(countMiss, (int)(Math.Floor((beatmapMaxCombo - 0.1 * countSliders) / scoreMaxCombo)));


            // Get player's throughput according to combo
            int comboTPCount = Attributes.ComboTPs.Length;
            var comboPercentages = Generate.LinearSpaced(comboTPCount, 1.0 / comboTPCount, 1);

            double scoreComboPercentage = ((double)scoreMaxCombo) / beatmapMaxCombo;
            double comboTP = LinearSpline.InterpolateSorted(comboPercentages, Attributes.ComboTPs)
                             .Interpolate(scoreComboPercentage);


            // Get player's throughput according to miss count
            double missTP;
            if (effectiveMissCount == 0)
                missTP = Attributes.MissTPs[0];
            else
            {
                missTP = LinearSpline.InterpolateSorted(Attributes.MissCounts, Attributes.MissTPs)
                                 .Interpolate(effectiveMissCount);
                missTP = Math.Max(missTP, 0);
            }

            // Combine combo based throughput and miss count based throughput
            double tp = Math.Pow(comboTP, comboWeight) * Math.Pow(missTP, 1 - comboWeight);

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
            double cheeseLevel = SpecialFunctions.Logistic(((urOnCheeseNotes * Attributes.AimDiff) - 3200) / 2000);
            double cheeseFactor = LinearSpline.InterpolateSorted(Attributes.CheeseLevels, Attributes.CheeseFactors)
                                  .Interpolate(cheeseLevel);



            if (mods.Any(m => m is OsuModTouchDevice))
                tp = Math.Min(tp, 1.47 * Math.Pow(tp, 0.8));

            double aimValue = tpToPP(tp * cheeseFactor);

            // penalize misses
            aimValue *= Math.Pow(0.96, effectiveMissCount);

            // Buff very high AR and low AR
            double approachRateFactor = 1.0;
            if (Attributes.ApproachRate > 10)
                approachRateFactor += (0.05 + 0.35 * Math.Pow(Math.Sin(Math.PI * Math.Min(totalHits, 1250) / 2500), 2.5)) *
                                      Math.Pow(Attributes.ApproachRate - 10, 2);
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
            double accLeniency = greatWindow * Attributes.AimDiff / 300;
            double accPenalty = (0.09 / (accuracy - 1.3) + 0.3) * (accLeniency + 1.5);
            aimValue *= Math.Exp(-accPenalty);

            return aimValue;
        }

        private double computeTapValue()
        {

            double modifiedAcc = getModifiedAcc();

            // Assume SS for non-stream parts
            double accOnStreams = 1 - (1 - modifiedAcc) * Math.Sqrt(totalHits / Attributes.StreamNoteCount);

            // accOnStreams can be negative. The formula below ensures a positive acc while
            // preserving the value when accOnStreams is close to 1
            double accOnStreamsPositive = Math.Exp(accOnStreams - 1);

            double urOnStreams = 10 * greatWindow / (Math.Sqrt(2) * SpecialFunctions.ErfInv(accOnStreamsPositive));

            double mashLevel = SpecialFunctions.Logistic(((urOnStreams * Attributes.TapDiff) - 4000) / 1000);
            
            double tapSkill = LinearSpline.InterpolateSorted(Attributes.MashLevels, Attributes.TapSkills)
                              .Interpolate(mashLevel);

            double tapValue = tapSkillToPP(tapSkill);

            // Buff very high acc on streams
            double accBuff = Math.Exp((accOnStreams - 1) * 60) * tapValue * 0.2;
            tapValue += accBuff;

            // Scale tap value down with accuracy
            double accFactor = 0.5 + 0.5 * (SpecialFunctions.Logistic((accuracy - 0.65) / 0.1) + SpecialFunctions.Logistic(-3.5));
            tapValue *= accFactor;

            // Penalize misses and 50s exponentially
            tapValue *= Math.Pow(0.93, countMiss);
            tapValue *= Math.Pow(0.98, countMeh);

            // Buff very high AR
            double approachRateFactor = 1.0;
            double ar11lengthBuff = 0.8 * (SpecialFunctions.Logistic(totalHits / 500) - 0.5);
            if (Attributes.ApproachRate > 10.33)
                approachRateFactor += ar11lengthBuff * (Attributes.ApproachRate - 10.33) / 0.67;

            tapValue *= approachRateFactor;

            return tapValue;
        }

        private double computeAccuracyValue()
        {
            double fingerControlDiff = Attributes.FingerControlDiff;

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

            double lengthFactor = SpecialFunctions.Logistic(Attributes.Length / 60.0);
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
            double modifiedAcc = ((countGreat - (totalHits - countHitCircles)) * 3 + countGood * 2 + countMeh) /
                                 ((countHitCircles + 2) * 3);
            return modifiedAcc;
        }

        private double tpToPP(double tp) => Math.Pow(tp, skillToPPExponent) * 0.129;

        private double tapSkillToPP(double tapSkill) => Math.Pow(tapSkill, skillToPPExponent) * 0.115;

        private double fingerControlDiffToPP(double fingerControlDiff) => Math.Pow(fingerControlDiff, skillToPPExponent);

        private double totalHits => countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulHits => countGreat + countGood + countMeh;
    }
}
