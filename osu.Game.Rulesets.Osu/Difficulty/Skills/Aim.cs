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
        protected override double StarsPerDouble => 1.15;
        protected override int HistoryLength => 2;

        private int decayExcessThreshold = 500;

        private double currSnapStrain = 1;
        private double currFlowStrain = 1;

        private double snapStrainMultiplier = 16.5;
        private double flowStrainMultiplier = 15;
        private double totalStrainMultiplier = .155;

        private Tapping tappingSkill;

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

        private double flowStrainAt(Vector2 prevVector, Vector2 currVector, Vector2 nextVector)
        {
            Vector2 nextDiffVector = Vector2.Subtract(currVector, nextVector);
            Vector2 prevDiffVector = Vector2.Subtract(prevVector, currVector);

            double strain = 2 * currVector.Length + prevDiffVector.Length + Math.Abs(prevDiffVector.Length - nextDiffVector.Length);

            return strain;
        }

        private double snapStrainAt(Vector2 prevVector, Vector2 currVector, Vector2 nextVector, double currStrainTime)
        {
            Vector2 nextDiffVector = Vector2.Add(currVector, nextVector);
            Vector2 prevDiffVector = Vector2.Add(prevVector, currVector);

            double strain = 2 * currVector.Length + prevDiffVector.Length + Math.Abs(prevDiffVector.Length - nextDiffVector.Length);

            //strain *= currStrainTime / (currStrainTime - 15);

            return strain;
        }

        private double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double strain = 0;

            double tappingStrain = tappingSkill.TappingStrain / 1000;

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

                double snapStrain = snapStrainAt(Vector2.Multiply(prevVector, (float)osuPrevObj.SnapProbability),
                                                 Vector2.Multiply(currVector, (float)osuCurrObj.SnapProbability),
                                                 Vector2.Multiply(prevVector, (float)osuNextObj.SnapProbability),
                                                 osuCurrObj.StrainTime);

                double flowStrain = flowStrainAt(Vector2.Multiply(prevVector, (float)osuPrevObj.FlowProbability),
                                                 Vector2.Multiply(currVector, (float)osuCurrObj.FlowProbability),
                                                 Vector2.Multiply(prevVector, (float)osuNextObj.FlowProbability));

                currSnapStrain *= computeDecay(.5, Math.Max(50, osuCurrObj.StrainTime));// - osuCurrObj.TravelTime));
                currSnapStrain += snapStrain * snapStrainMultiplier;

                currFlowStrain *= computeDecay(.75, Math.Max(50, osuCurrObj.StrainTime));// - osuCurrObj.TravelTime));
                currFlowStrain += flowStrain * flowStrainMultiplier;

                strain = totalStrainMultiplier * Math.Max(currSnapStrain, currFlowStrain);

                strain *= osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 15);
            }



            // Console.WriteLine(tappingStrain);

            return strain;// * Math.Sqrt(4 + tappingStrain) / 2;
        }

        public void SetTappingSkill(Tapping tapping) => tappingSkill = tapping;

        protected override void Process(DifficultyHitObject current)
        {
            AddStrain(strainValueAt(current));
        }
    }
}
