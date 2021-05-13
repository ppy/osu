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

        private double currSnapStrain = 1;
        private double currFlowStrain = 1;

        private double snapStrainMultiplier = 11.5;
        private double flowStrainMultiplier = 20;
        private double totalStrainMultiplier = .1225;

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

            double strain = 2 * currVector.Length + Math.Min(currVector.Length, prevDiffStrain) + Math.Abs(prevDiffStrain - nextDiffStrain) * Math.Min(osuNextObj.FlowProbability, osuPrevObj.FlowProbability);

            // strain = Math.Pow(strain, 1.1);

            return strain;
        }

        private double snapStrainAt(OsuDifficultyHitObject osuPrevObj, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuNextObj,
                                    Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            double nextDiffStrain = Vector2.Add(currVector, nextVector).Length * osuNextObj.SnapProbability;
            double prevDiffStrain = Vector2.Add(prevVector, currVector).Length * osuPrevObj.SnapProbability;

            double strain = 2 * currVector.Length + prevDiffStrain + Math.Abs(prevDiffStrain - nextDiffStrain) * Math.Min(osuNextObj.SnapProbability, osuPrevObj.SnapProbability);

            strain *= osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 20);

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

                double flowProb = osuCurrObj.FlowProbability;
                double snapProb = osuCurrObj.SnapProbability;

                double snapStrain = snapStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 Vector2.Multiply(prevVector, (float)osuPrevObj.SnapProbability),
                                                 Vector2.Multiply(currVector, (float)osuCurrObj.SnapProbability),
                                                 Vector2.Multiply(prevVector, (float)osuNextObj.SnapProbability));

                double flowStrain = flowStrainAt(osuPrevObj,
                                                 osuCurrObj,
                                                 osuNextObj,
                                                 Vector2.Multiply(prevVector, (float)osuPrevObj.FlowProbability),
                                                 Vector2.Multiply(currVector, (float)osuCurrObj.FlowProbability),
                                                 Vector2.Multiply(prevVector, (float)osuNextObj.FlowProbability));

                currSnapStrain *= computeDecay(.66, Math.Max(50, osuCurrObj.StrainTime));// - osuCurrObj.TravelTime));
                currSnapStrain += snapStrain * snapStrainMultiplier;

                currFlowStrain *= computeDecay(.75, Math.Max(50, osuCurrObj.StrainTime));// - osuCurrObj.TravelTime));
                currFlowStrain += flowStrain * flowStrainMultiplier;

                strain = totalStrainMultiplier * (Math.Max(currFlowStrain, currSnapStrain) + Math.Min(currFlowStrain, currSnapStrain));
      // (Math.Max(currFlowStrain, currSnapStrain) + Math.Min(currFlowStrain, currSnapStrain));
    // (currFlowStrain + currSnapStrain);
    // Math.Pow(Math.Pow(currSnapStrain, hybridScaler) + Math.Pow(currFlowStrain, hybridScaler), 1.0 / hybridScaler);
            }



            // Console.WriteLine(tappingStrain);

            return strain;// * Math.Sqrt(4 + tappingStrain) / 2;
        }

        public void SetTapSkill(Tap tap) => tapSkill = tap;

        protected override void Process(DifficultyHitObject current)
        {
            AddStrain(strainValueAt(current));
        }
    }
}
