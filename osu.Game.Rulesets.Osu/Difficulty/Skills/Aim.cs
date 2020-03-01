﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using static System.Math;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : Skill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;
        private const double streamaimconst = 2.42;
        private const double stdevconst = 0.149820;
        double[] JumpDistanceArray;
        int jumpdistpointercount = 0;

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        /// Standard Deviation Calculation Code courtesy of Roman http://csharphelper.com/blog/2015/12/make-an-extension-method-that-calculates-standard-deviation-in-c/
        /// public static double GetStandardDeviation(List values)
        ///     {
        ///     double avg = values.Average();
        ///     double sum = values.Sum(v => (v - avg) * (v-avg));
        ///     double denominator = values.Count - 1;
        ///     return denominator > 0.0 ? Math.Sqrt(sum / denominator) : -1;
        ///     };
        public static double StreamDeterminant(double deltadist,double deltatime)
            {
               double sectionvelocity = deltadist/deltatime;
               double[] JumpDistanceArray;
               int jumpdistpointercount = 0;
               if (sectionvelocity < streamaimconst) 
                { 
                    JumpDistanceArray[jumpdistpointercount] = deltadist;
                    int jumpdistpointercount = jumpdistpointercount + 1;
                /// Use JumpDistanceArray to calculate SD
                /// https://stackoverflow.com/a/5336708
                /// double average = JumpDistanceArray.Average();
                /// double sumOfSquaresOfDifferences = JumpDistanceArray.Select(val => (val - average) * (val - average)).Sum();
                /// double finalsd = Math.Sqrt(sumOfSquaresOfDifferences / JumpDistanceArray.Length); 
                /// double finalsdpp = finalsd/stdevconst when missCount = 0;
                }
            }
        
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double result = 0;

            StreamDeterminant(osuCurrent.JumpDistance,osuCurrent.StrainTime);

            if (Previous.Count > 0)
            {
                var osuPrevious = (OsuDifficultyHitObject)Previous[0];

                if (osuCurrent.Angle != null && osuCurrent.Angle.Value > angle_bonus_begin)
                {
                    const double scale = 90;

                    var angleBonus = Math.Sqrt(
                        Math.Max(osuPrevious.JumpDistance - scale, 0)
                        * Math.Pow(Math.Sin(osuCurrent.Angle.Value - angle_bonus_begin), 2)
                        * Math.Max(osuCurrent.JumpDistance - scale, 0));
                    result = 1.5 * applyDiminishingExp(Math.Max(0, angleBonus)) / Math.Max(timing_threshold, osuPrevious.StrainTime);
                }
            }

            double jumpDistanceExp = applyDiminishingExp(osuCurrent.JumpDistance);
            double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance);
            
            return Math.Max(
                result + (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(osuCurrent.StrainTime, timing_threshold),
                (Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / osuCurrent.StrainTime
            );
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
