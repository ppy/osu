// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuSkill
    {
        protected override double StarsPerDouble => 1.1;
        protected override int HistoryLength => 2;

        private int decayExcessThreshold = 500;

        private double currStrain = 1;

        private double snapStrainMultiplier = 9.125;
        private double flowStrainMultiplier = 16.25;
        private double hybridStrainMultiplier = 8.25;
        private double sliderStrainMultiplier = 75;
        private double totalStrainMultiplier = .1675;

        private Tap tapSkill;

        public Aim(Mod[] mods)
            : base(mods)
        {
        }

        private double computeDecay(double baseDecay, double ms)
        {
            double decay = 0;
            if (ms < decayExcessThreshold)
                decay = baseDecay;
            else
                decay = Math.Pow(Math.Pow(baseDecay, 1000 / Math.Min(ms, decayExcessThreshold)), ms / 1000);

            return decay;
        }

        private double flowStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            var nextDiffVector = Vector2.Subtract(currVector, nextVector);
            var prevDiffVector = Vector2.Subtract(prevVector, currVector);

            double minDistance = 0;

            double angle = (osuCurrObj.Angle + osuNextObj.Angle) / 2;

            double minBoyDistance = Math.Min(currVector.Length, prevVector.Length);

            if (angle < Math.PI / 4)
                minDistance = 0;
            else if (angle > Math.PI / 2)
                minDistance = Math.Max(0, Math.Min(100 / osuCurrObj.StrainTime, prevDiffVector.Length - Math.Max(currVector.Length, prevVector.Length) / 2));
            else
                minDistance = Math.Min(1, Math.Max(0, osuCurrObj.JumpDistance - 75) / 50)
                              * Math.Pow(Math.Sin(2 * (angle - Math.PI / 4)), 2)
                              * Math.Max(0, Math.Min(Math.Min(100 / osuCurrObj.StrainTime, minBoyDistance), prevDiffVector.Length - Math.Max(currVector.Length, prevVector.Length) / 2));

            double strain = prevVector.Length * osuPrevObj.FlowProbability
                          + currVector.Length * osuCurrObj.FlowProbability
                          + Math.Min(Math.Min(currVector.Length, prevVector.Length), Math.Abs(currVector.Length - prevVector.Length)) * osuCurrObj.FlowProbability * osuPrevObj.FlowProbability
                          + minDistance * osuCurrObj.FlowProbability * osuPrevObj.FlowProbability;

            strain *= Math.Min(osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 10) , osuPrevObj.StrainTime / (osuPrevObj.StrainTime - 10));

            return strain;
        }

        private double snapScaling(double distance)
        {
            if (distance == 0)
                return 0;
            else
                return (5 * (Math.Log(distance / 5 + 1) / Math.Log(2))) / distance;
        }

        private double snapStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            currVector = Vector2.Divide(Vector2.Multiply(osuCurrObj.DistanceVector, (float)snapScaling(osuCurrObj.JumpDistance / 104)), (float)osuCurrObj.StrainTime);
            prevVector = Vector2.Divide(Vector2.Multiply(osuPrevObj.DistanceVector, (float)snapScaling(osuPrevObj.JumpDistance / 104)), (float)osuPrevObj.StrainTime);

            var nextDiffVector = Vector2.Add(currVector, nextVector);
            var prevDiffVector = Vector2.Add(prevVector, currVector);

            double angleDistance = Math.Max(0, prevDiffVector.Length - Math.Max(currVector.Length, prevVector.Length));

            double angleAdjustment = 0;

            double currDistance = currVector.Length * osuCurrObj.SnapProbability + prevVector.Length * osuPrevObj.SnapProbability;

            double angle = Math.Abs(osuCurrObj.Angle);

            if (angle < Math.PI / 3)
            {
                angleAdjustment -= .2 * Math.Abs(currDistance - angleDistance) * Math.Pow(Math.Sin(Math.PI / 2 - angle * 1.5), 2);
            }
            else
            {
                angleAdjustment += angleDistance * (1 + .5 * Math.Pow(Math.Sin(angle - Math.PI / 4), 2));
            }

            double strain = currDistance + angleAdjustment * osuCurrObj.SnapProbability * osuPrevObj.SnapProbability;

            strain *= Math.Min(osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 20) , osuPrevObj.StrainTime / (osuPrevObj.StrainTime - 20));

            return strain;
        }

        private double hybridStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                      Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            var flowToSnapVector = Vector2.Subtract(prevVector, currVector);
            var snapToFlowVector = Vector2.Add(currVector, nextVector);

            double flowToSnapStrain = flowToSnapVector.Length * osuCurrObj.SnapProbability * osuPrevObj.FlowProbability;
            double snapToFlowStrain = snapToFlowVector.Length * osuCurrObj.SnapProbability * osuNextObj.FlowProbability;

            double strain = Math.Max(Math.Sqrt(flowToSnapStrain * Math.Sqrt(currVector.Length * prevVector.Length)), Math.Sqrt(snapToFlowStrain * Math.Sqrt(currVector.Length * nextVector.Length)));

            return strain;
        }

        private double sliderStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj)
        {
            double strain = (osuPrevObj.TravelDistance) / osuPrevObj.StrainTime;

            return strain;
        }

        protected override double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double strain = 0;

            if (Previous.Count > 1)
            {
                var osuNextObj = (OsuDifficultyHitObject)current;
                var osuCurrObj = (OsuDifficultyHitObject)Previous[0];
                var osuPrevObj = (OsuDifficultyHitObject)Previous[1];

                Vector2 nextVector = Vector2.Divide(osuNextObj.DistanceVector, (float)osuNextObj.StrainTime);
                Vector2 currVector = Vector2.Divide(osuCurrObj.DistanceVector, (float)osuCurrObj.StrainTime);
                Vector2 prevVector = Vector2.Divide(osuPrevObj.DistanceVector, (float)osuPrevObj.StrainTime);

                double snapStrain = snapStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 prevVector,
                                                 currVector,
                                                 prevVector);

                double flowStrain = flowStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 prevVector,
                                                 currVector,
                                                 prevVector);

                double hybridStrain = hybridStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj,
                                                     prevVector,
                                                     currVector,
                                                     prevVector);

                double sliderStrain = sliderStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj);


                currStrain *= computeDecay(.75, Math.Max(50, osuCurrObj.StrainTime));
                currStrain += snapStrain * snapStrainMultiplier;
                currStrain += flowStrain * flowStrainMultiplier;
                currStrain += hybridStrain * hybridStrainMultiplier;
                currStrain += sliderStrain * sliderStrainMultiplier;

                strain = totalStrainMultiplier * currStrain;
            }

            return strain;
        }

        public void SetTapSkill(Tap tap) => tapSkill = tap;
    }
}
