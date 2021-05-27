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
        private double distanceConstant = 3.5;

        // Global Constants for the different types of aim.
        private double snapStrainMultiplier = 23.727;
        private double flowStrainMultiplier = 24.727;
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
            double strain = 0;

            var observedDistance = Vector2.Subtract(currVector, Vector2.Multiply(prevVector, (float)0.1));

            double prevAngularMomentumChange = Math.Abs(osuCurrObj.Angle * currVector.Length - osuPrevObj.Angle * prevVector.Length);
            double nextAngularMomentumChange = Math.Abs(osuCurrObj.Angle * currVector.Length - osuNextObj.Angle * nextVector.Length);

            double angularMomentumChange = Math.Sqrt(Math.Min(currVector.Length, prevVector.Length) * Math.Abs(nextAngularMomentumChange - prevAngularMomentumChange) / (2 * Math.PI));

            double momentumChange = Math.Sqrt(Math.Abs(currVector.Length - prevVector.Length) * Math.Min(currVector.Length, prevVector.Length));

            strain = osuCurrObj.FlowProbability * (observedDistance.Length
                                                    + momentumChange * (0.5 + 0.5 * osuPrevObj.FlowProbability)
                                                    + angularMomentumChange * osuPrevObj.FlowProbability);

            strain *= Math.Min(osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 20) , osuPrevObj.StrainTime / (osuPrevObj.StrainTime - 20));
            // buff high BPM slightly.

            return strain;
        }

        /// <summary>
        /// Alters the distance traveled for snapping to match the results from Fitt's law.
        /// </summary>
        private double snapScaling(double distance)
        {
            if (distance <= distanceConstant)
                return 1;
            else
                return (distanceConstant + (distance - distanceConstant) * (Math.Log(1 + (distance - distanceConstant) / Math.Sqrt(2)) / Math.Log(2)) / (distance - distanceConstant)) / distance;
        }

        /// <summary>
        /// Calculates the difficulty to snap from the previous <see cref="OsuDifficultyHitObject"/> the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double snapStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            double strain = 0;

            currVector = Vector2.Multiply(currVector, (float)snapScaling(osuCurrObj.JumpDistance / 100));
            prevVector = Vector2.Multiply(prevVector, (float)snapScaling(osuPrevObj.JumpDistance / 100));

            var observedDistance = Vector2.Add(currVector, Vector2.Multiply(prevVector, (float)(0.35)));

            strain = observedDistance.Length * osuCurrObj.SnapProbability;

            strain *= Math.Min(osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 20) , osuPrevObj.StrainTime / (osuPrevObj.StrainTime - 20));
            // buff high BPM slightly.

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

                double sliderStrain = sliderStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj);
                // Currently passing all available data, just incase it is useful for calculation.

                currStrain *= computeDecay(baseDecay, osuCurrent.StrainTime);
                currStrain += snapStrain * snapStrainMultiplier;
                currStrain += flowStrain * flowStrainMultiplier;
                currStrain += sliderStrain * sliderStrainMultiplier;

                strain = totalStrainMultiplier * currStrain;
            }

            return strain;
        }
    }
}
