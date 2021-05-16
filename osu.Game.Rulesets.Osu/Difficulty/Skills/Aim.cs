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
        protected override double StarsPerDouble => 1.125;
        protected override int HistoryLength => 2;

        private int decayExcessThreshold = 500;

        private double currStrain = 1;

        private double snapStrainMultiplier = 17.0;
        private double flowStrainMultiplier = 18.5;
        private double hybridStrainMultiplier = 0;
        private double sliderStrainMultiplier = 30;
        private double totalStrainMultiplier = .175;

        private int curr = 0;

        // private double hybridScaler = 1.5;

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
            double nextDiffStrain = Vector2.Subtract(currVector, nextVector).Length * osuNextObj.FlowProbability;
            double prevDiffStrain = Vector2.Subtract(prevVector, currVector).Length * osuPrevObj.FlowProbability;

            double minDistance = 0;

            if (prevDiffStrain > currVector.Length && nextDiffStrain > currVector.Length)
                minDistance = Math.Min(1, Math.Max(0, (osuCurrObj.JumpDistance - 75)) / 30) * Math.Min(prevDiffStrain, 150 / osuCurrObj.StrainTime);
            else
                minDistance = Math.Min(prevDiffStrain, 150 / osuCurrObj.StrainTime);

            double strain = 2 * currVector.Length + minDistance
                    + Math.Abs(prevDiffStrain - nextDiffStrain) * Math.Min(osuNextObj.FlowProbability, osuPrevObj.FlowProbability);

            // strain = Math.Pow(strain, 1.1);

            return strain;
        }

        private double snapStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            double nextDiffStrain = Vector2.Add(currVector, nextVector).Length;
            double prevDiffStrain = Vector2.Add(prevVector, currVector).Length;

            double strain = 100 / osuCurrObj.StrainTime + currVector.Length
 + Math.Max(0, prevDiffStrain - (Math.Max(currVector.Length, prevVector.Length) + Math.Abs(currVector.Length - prevVector.Length) / 2) / 2) / 1.5
+ Math.Abs(prevDiffStrain - nextDiffStrain) * Math.Min(osuNextObj.SnapProbability, osuPrevObj.SnapProbability);

            strain *= osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 20);

            return strain;
        }

        private double hybridStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                      Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            double flowToSnap = Math.Sqrt(prevVector.Length * Math.Min(currVector.Length, 150 / osuCurrObj.StrainTime)) * osuCurrObj.SnapProbability * osuPrevObj.FlowProbability;
            double snapToFlow = Math.Sqrt(Math.Min(prevVector.Length, 150 / osuPrevObj.StrainTime) * currVector.Length) * osuPrevObj.SnapProbability * osuCurrObj.FlowProbability;

            double strain = Math.Max(flowToSnap, snapToFlow);

            return strain;
        }

        private double sliderStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj)
        {
            double strain = (Math.Sqrt(osuCurrObj.JumpDistance * osuCurrObj.TravelDistance) + osuCurrObj.TravelDistance) / osuCurrObj.StrainTime;

            return strain;
        }

        private double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double strain = 0;

            double tapStrain = tapSkill.TapStrain / 1000;

            if (Previous.Count > 1)
            {
                var osuNextObj = (OsuDifficultyHitObject)current;
                var osuCurrObj = (OsuDifficultyHitObject)Previous[0];
                var osuPrevObj = (OsuDifficultyHitObject)Previous[1];

                Vector2 nextVector = Vector2.Divide(osuNextObj.DistanceVector, (float)osuNextObj.StrainTime);
                Vector2 currVector = Vector2.Divide(osuCurrObj.DistanceVector, (float)osuCurrObj.StrainTime);
                Vector2 prevVector = Vector2.Divide(osuPrevObj.DistanceVector, (float)osuPrevObj.StrainTime);

                double snapStrain = osuCurrObj.SnapProbability *
                                    snapStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 prevVector,
                                                 currVector,
                                                 prevVector);

                double flowStrain = osuCurrObj.FlowProbability *
                                    flowStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 prevVector,
                                                 currVector,
                                                 prevVector);

                double hybridStrain = (1 - 4 * Math.Max(osuPrevObj.FlowProbability * osuPrevObj.SnapProbability, osuCurrObj.SnapProbability * osuCurrObj.FlowProbability)) *
                                      hybridStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj,
                                                     prevVector,
                                                     currVector,
                                                     prevVector);

                double sliderStrain = sliderStrainAt(osuPrevObj,
                                                     osuCurrObj,
                                                     osuNextObj);


                currStrain *= computeDecay(.75, Math.Max(50, osuCurrObj.StrainTime));// - osuCurrObj.TravelTime));
                currStrain += snapStrain * snapStrainMultiplier * Math.Sqrt(1 + tapSkill.TapStrain / 400);
                currStrain += flowStrain * flowStrainMultiplier * Math.Sqrt(1 + tapSkill.TapStrain / 150);
                currStrain += hybridStrain * hybridStrainMultiplier;
                currStrain += sliderStrain * sliderStrainMultiplier;


                // Console.WriteLine("C: " + curr);
                // Console.WriteLine("Strain: " + currStrain);
                // curr++;

                strain = totalStrainMultiplier * currStrain;
      // (Math.Max(currFlowStrain, currSnapStrain) + Math.Min(currFlowStrain, currSnapStrain));
    // (currFlowStrain + currSnapStrain);
    // Math.Pow(Math.Pow(currSnapStrain, hybridScaler) + Math.Pow(currFlowStrain, hybridScaler), 1.0 / hybridScaler);
            }


  curr++;
            // Console.WriteLine(tappingStrain);

            return strain;// * Math.Sqrt(4 + tappingStrain) / 2;
        }

        public void SetTapSkill(Tap tap) => tapSkill = tap;

        protected override void Process(DifficultyHitObject current)
        {
            double strain = strainValueAt(current);

            // strain *= Math.Sqrt(1 + tapSkill.TapStrain / 100);

            // Console.WriteLine(Math.Sqrt(1 + tapSkill.TapStrain / 100));

            AddStrain(strain);
        }
    }
}
