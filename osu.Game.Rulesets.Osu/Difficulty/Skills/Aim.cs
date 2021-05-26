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
        protected override int decayExcessThreshold => 500;
        protected override double baseDecay => 0.75;

        private double currStrain = 1;
        private double fittsSnapConstant = 3.75;

        // Global Constants for the different types of aim.
        private double snapStrainMultiplier = 9.875;
        private double flowStrainMultiplier = 16.25;
        private double hybridStrainMultiplier = 8.25;
        private double sliderStrainMultiplier = 75;
        private double totalStrainMultiplier = .1675;

        public Aim(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Calculates the difficulty to flow from the previous <see cref="OsuDifficultyHitObject"/> the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double flowStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            var prevDiffVector = Vector2.Subtract(prevVector, currVector);
            // we want to award acute angles when it comes to flow, its hard to flow a wiggle pattern.

            double angleAdjustment = 0;

            double angle = (osuCurrObj.Angle + osuNextObj.Angle) / 2; // average the angles, we want to award acute stream cuts, but not when they're back and forths.

            double minDistance = Math.Min(currVector.Length, prevVector.Length);

            if (angle < Math.PI / 4) // angles less than 45, we assume overlap and dont award. (these angles are usually basically snaps, but at extremely high bpm)
                angleAdjustment = 0;
            else if (angle > Math.PI / 2) // angles greater than 90, we want to buff for having tighter angles (more curve in stream)
                angleAdjustment = Math.Max(0, Math.Min(100 / osuCurrObj.StrainTime,
                                                      prevDiffVector.Length - Math.Max(currVector.Length, prevVector.Length) / 2));
            else // we want to do the same acute angle buffs as above, but slowly transition down to 0 with a 45 degree angle.
                angleAdjustment = Math.Min(1, Math.Max(0, osuCurrObj.JumpDistance - 75) / 50)
                                  * Math.Pow(Math.Sin(2 * (angle - Math.PI / 4)), 2)
                                  * Math.Max(0, Math.Min(100 / osuCurrObj.StrainTime,
                                                     prevDiffVector.Length - Math.Max(currVector.Length, prevVector.Length) / 2));

            double strain = prevVector.Length * osuPrevObj.FlowProbability
                          + currVector.Length * osuCurrObj.FlowProbability
                          + Math.Min(Math.Min(currVector.Length, prevVector.Length), Math.Abs(currVector.Length - prevVector.Length)) * osuCurrObj.FlowProbability * osuPrevObj.FlowProbability
                          + angleAdjustment * osuCurrObj.FlowProbability * osuPrevObj.FlowProbability;
            // here we have the velocities of curr, prev, the difference between them, and our angle buff.

            strain *= Math.Min(osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 10) , osuPrevObj.StrainTime / (osuPrevObj.StrainTime - 10));
            // buff high BPM slightly.

            return strain;
        }

        /// <summary>
        /// Alters the distance traveled for snapping to match the results from Fitt's law.
        /// </summary>
        private double snapScaling(double distance)
        {
            if (distance == 0)
                return 0;
            else
                return (fittsSnapConstant * (Math.Log(distance / fittsSnapConstant + 1) / Math.Log(2))) / distance;
        }

        /// <summary>
        /// Calculates the difficulty to snap from the previous <see cref="OsuDifficultyHitObject"/> the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double snapStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            currVector = Vector2.Divide(Vector2.Multiply(osuCurrObj.DistanceVector, (float)snapScaling(osuCurrObj.JumpDistance / 100)), (float)osuCurrObj.StrainTime);
            prevVector = Vector2.Divide(Vector2.Multiply(osuPrevObj.DistanceVector, (float)snapScaling(osuPrevObj.JumpDistance / 100)), (float)osuPrevObj.StrainTime);
            // Here we want to scale vectors with fitt's law, so we have to recreate them...

            var prevDiffVector = Vector2.Add(prevVector, currVector);
            double angleDistance = Math.Max(0, prevDiffVector.Length - Math.Max(currVector.Length, prevVector.Length));
            // We want to award wide angles, so we add the vectors, and then subtract the largest vector out to get a distance beyond 60 degrees.

            double angleAdjustment = 0;

            double currDistance = currVector.Length * osuCurrObj.SnapProbability + prevVector.Length * osuPrevObj.SnapProbability;

            double angle = Math.Abs(osuCurrObj.Angle);

            if (angle < Math.PI / 3)
            {
                angleAdjustment -= .2 * Math.Abs(currDistance - angleDistance) * Math.Pow(Math.Sin(Math.PI / 2 - angle * 1.5), 2); // penalize acute angles in snapping.
            }
            else
            {
                angleAdjustment += angleDistance * (1 + .5 * Math.Pow(Math.Sin(angle - Math.PI / 4), 2)); // buff wide angles, especially in the 90 degree range.
            }

            double strain = currDistance
                            + angleAdjustment * osuCurrObj.SnapProbability * osuPrevObj.SnapProbability;

            strain *= Math.Min(osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 20) , osuPrevObj.StrainTime / (osuPrevObj.StrainTime - 20));
            // buff high BPM slightly.

            return strain;
        }

        /// <summary>
        /// Calculates the interactional difficulty associated with transitioning from flow to snap or from snap to flow for the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double hybridStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                      Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            var flowToSnapVector = Vector2.Subtract(prevVector, currVector);
            var snapToFlowVector = Vector2.Add(currVector, nextVector);

            double flowToSnapStrain = flowToSnapVector.Length * osuCurrObj.SnapProbability * osuPrevObj.FlowProbability;
            double snapToFlowStrain = snapToFlowVector.Length * osuCurrObj.SnapProbability * osuNextObj.FlowProbability;

            double strain = Math.Max(Math.Sqrt(flowToSnapStrain * Math.Sqrt(currVector.Length * prevVector.Length)),
                                     Math.Sqrt(snapToFlowStrain * Math.Sqrt(currVector.Length * nextVector.Length)));

            // Here we are taking a geometric average of the two vectors associated with our calculation, with their vector's interaction.
            // We want to buff wide angles for snap to flow, and buff acute angles for flow to snap.

            return strain;
        }

        /// <summary>
        /// Calculates the estimated difficulty associated with the slider movement from the previous <see cref="OsuDifficultyHitObject"/> to the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
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
                // Since it is easier to get history, we take the previous[0] as our current, so we can see our "next"

                Vector2 nextVector = Vector2.Divide(osuNextObj.DistanceVector, (float)osuNextObj.StrainTime);
                Vector2 currVector = Vector2.Divide(osuCurrObj.DistanceVector, (float)osuCurrObj.StrainTime);
                Vector2 prevVector = Vector2.Divide(osuPrevObj.DistanceVector, (float)osuPrevObj.StrainTime);

                double snapStrain = snapStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 prevVector,
                                                 currVector,
                                                 nextVector);

                double flowStrain = flowStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 prevVector,
                                                 currVector,
                                                 nextVector);

                double hybridStrain = hybridStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj,
                                                     prevVector,
                                                     currVector,
                                                     nextVector);

                double sliderStrain = sliderStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj);
                // Currently passing all available data, just incase it is useful for calculation.

                currStrain *= computeDecay(baseDecay, osuCurrent.StrainTime);
                currStrain += snapStrain * snapStrainMultiplier;
                currStrain += flowStrain * flowStrainMultiplier;
                currStrain += hybridStrain * hybridStrainMultiplier;
                currStrain += sliderStrain * sliderStrainMultiplier;

                strain = totalStrainMultiplier * currStrain;
            }

            return strain;
        }
    }
}
