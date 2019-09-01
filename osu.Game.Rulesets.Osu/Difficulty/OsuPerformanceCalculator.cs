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
        private const double fcBuffFactor = 0.1;

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

            // Don't count scores made with supposedly unranked mods
            if (mods.Any(m => !m.Ranked))
                return 0;

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 1.7; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= 0.90f;

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 0.95f;

            double aimValue = computeAimValue();
            double speedValue = computeSpeedValue();
            double totalValue = Mean.PowerMean(aimValue, speedValue, totalValueExponent) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Speed", speedValue);
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", beatmapMaxCombo);
            }

            return totalValue;
        }

        private double computeAimValue()
        {

            // Guess the number of misaims from combo
            int effectiveMissCount = Math.Max(countMiss, (int)(Math.Floor(0.9 * beatmapMaxCombo / scoreMaxCombo)));

            // Get player's throughput. Interpolate if there are misses.
            double tp;
            if (effectiveMissCount == 0)
                tp = Attributes.MissTPs[0];
            else
            {
                tp = LinearSpline.InterpolateSorted(Attributes.MissCounts, Attributes.MissTPs)
                                 .Interpolate(effectiveMissCount);
                tp = Math.Max(tp, 0);
            }

            if (mods.Any(m => m is OsuModTouchDevice))
                tp = Math.Pow(tp, 0.8);

            // Scale tp according to cheese level and acc
            // Treat 300 as 300, 100 as 200, 50 as 100
            // add 1 to denominator so that later erf gives resonable result
            double modifiedAcc;
            if (countHitCircles > 0)
                modifiedAcc = ((countGreat - (totalHits - countHitCircles)) * 3 + countGood * 2 + countMeh) /
                              ((countHitCircles * 3) + 1);
            else
                modifiedAcc = 0;

            // Assume SS for non-stream parts
            double accOnCheeseNotes = 1 - (1 - modifiedAcc) * countHitCircles / Attributes.StreamNoteCount;

            // accOnStreams can be negative. The formula below ensures a positive acc while
            // preserving the value when accOnStreams is close to 1
            double accOnCheeseNotesPositive = Math.Exp(accOnCheeseNotes - 1);

            double urOnCheeseNotes = 10 * (80 - 6 * Attributes.OverallDifficulty) /
                                 (Math.Sqrt(2) * SpecialFunctions.ErfInv(accOnCheeseNotesPositive));

            double cheeseLevel = SpecialFunctions.Logistic(((urOnCheeseNotes * Attributes.AimDiff) - 2800) / 300);

            double cheeseFactor = LinearSpline.InterpolateSorted(Attributes.CheeseLevels, Attributes.CheeseFactors)
                                  .Interpolate(cheeseLevel);
            //Console.WriteLine(accOnCheeseNotes);
            //Console.WriteLine(urOnCheeseNotes * Attributes.AimDiff);
            //Console.WriteLine(cheeseLevel);

            double aimValue = Math.Pow(tp * cheeseFactor, 2.55) * 0.165;




            // Buff full combo scores
            double fcness;
            if (effectiveMissCount > 0)
                fcness = 0;
            else if (countSliders == 0)
                fcness = 1;
            else
            {
                // assuming missing slider tails because we cannot differentiate a slider break
                // near the beginning/end from a number of missed slider tails
                double sliderTailMissingRate = (double)(beatmapMaxCombo - scoreMaxCombo) / countSliders;
                fcness = 2 - 2 * SpecialFunctions.Logistic(sliderTailMissingRate * 50) ;
            }
            aimValue *= 1 + fcness * fcBuffFactor;


            // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
            if (mods.Any(h => h is OsuModHidden))
                aimValue *= 1.0f + 0.04f * (12.0f - Attributes.ApproachRate);

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply object-based bonus for flashlight.
                aimValue *= 1.0f + 0.35f * Math.Min(1.0f, totalHits / 200.0f) +
                            (totalHits > 200
                                ? 0.3f * Math.Min(1.0f, (totalHits - 200) / 300.0f) +
                                  (totalHits > 500 ? (totalHits - 500) / 1200.0f : 0.0f)
                                : 0.0f);
            }


            //// Scale the aim value with accuracy _slightly_
            //aimValue *= 0.5f + accuracy / 2.0f;
            //// It is important to also consider accuracy difficulty when doing that
            //aimValue *= 0.98f + Math.Pow(Attributes.OverallDifficulty, 2) / 2500;

            double accLeniency = (80 - 6 * Attributes.OverallDifficulty) * Attributes.AimDiff / 300;
            double accPenalty = (SpecialFunctions.Logistic((accuracy-0.94) * (-200.0/3)) - SpecialFunctions.Logistic(-4)) *
                                Math.Pow(accLeniency, 2) * 0.2;

            aimValue *= Math.Exp(-accPenalty);

            //Console.WriteLine(accLeniency);
            //Console.WriteLine(accPenalty);

            return aimValue;
        }

        private double computeSpeedValue()
        {
            // Treat 300 as 300, 100 as 200, 50 as 100
            // add 1 to denominator so that later erf gives resonable result
            double modifiedAcc;
            if (countHitCircles > 0)
                modifiedAcc = ((countGreat - (totalHits - countHitCircles)) * 3 + countGood * 2 + countMeh) /
                              ((countHitCircles * 3) + 1);
            else
                modifiedAcc = 0;
                
            // Assume SS for non-stream parts
            double accOnStreams = 1 - (1 - modifiedAcc) * countHitCircles / Attributes.StreamNoteCount;

            // accOnStreams can be negative. The formula below ensures a positive acc while
            // preserving the value when accOnStreams is close to 1
            double accOnStreamsPositive = Math.Exp(accOnStreams - 1);

            double urOnStreams = 10 * (80 - 6 * Attributes.OverallDifficulty) /
                                 (Math.Sqrt(2) * SpecialFunctions.ErfInv(accOnStreamsPositive));

            double mashLevel = SpecialFunctions.Logistic(((urOnStreams * Attributes.TapDiff) - 2700) / 600);
            


            double tapSkill = LinearSpline.InterpolateSorted(Attributes.MashLevels, Attributes.TapSkills)
                              .Interpolate(mashLevel);



            double tapValue = Math.Pow(tapSkill * 0.96, 2.55) * 0.37;

            // Buff high acc
            double accBuffLevel = (1 - SpecialFunctions.Logistic(((urOnStreams * Attributes.TapDiff) - 1000) / 500)) /
                                  SpecialFunctions.Logistic(1000.0 / 500);
            accBuffLevel = Math.Pow(accBuffLevel, 2) * 1.2;
            tapValue *= 1 + accBuffLevel;

            //Console.WriteLine(urOnStreams * Attributes.TapDiff);
            //Console.WriteLine(mashLevel);
            //Console.WriteLine(tapSkill);
            //Console.WriteLine(accBuffLevel);

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            tapValue *= Math.Pow(0.97f, countMiss);


            double approachRateFactor = 1.0f;
            if (Attributes.ApproachRate > 10.33f)
                approachRateFactor += 0.3f * (Attributes.ApproachRate - 10.33f);
            tapValue *= approachRateFactor;

            if (mods.Any(m => m is OsuModHidden))
                tapValue *= 1.0f + 0.04f * (12.0f - Attributes.ApproachRate);


            return tapValue;
        }

        private double totalHits => countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulHits => countGreat + countGood + countMeh;
    }
}
