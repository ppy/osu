// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Aggregation
{
    public abstract class OsuProbabilitySkill : OsuStrainSkill
    {
        protected OsuProbabilitySkill(Mod[] mods)
            : base(mods)
        {
        }

        // We assume players have a 2% chance to hit every note in the map.
        // A higher value of fc_probability increases the influence of difficulty spikes,
        // while a lower value increases the influence of length and consistent difficulty.
        protected abstract double FcProbability { get; }

        private const int bin_count = 32;

        // The number of difficulties there must be before we can be sure that binning difficulties would not change the output significantly.
        private double binThreshold => 2 * bin_count;

        /// <summary>
        /// Returns the strain value at <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
       // protected abstract double StrainValueAt(DifficultyHitObject current);

        //public override void Process(DifficultyHitObject current)
        //{
        //    Difficulties.Add(StrainValueAt(current));
        //}

        protected abstract double HitProbability(double skill, double difficulty);

        public bool UseProbabilityAdjust => true;
        public bool UseDefaultMissProb => false;
        public bool UseDifficultyPower => false;

        public const double LOW_SKILL_THRESHOLD = 1000;
        public const double MID_SKILL_THRESHOLD = 2500;

        // Reward consistency much more on low skill level
        private double probabilityAdjust(double skill) => UseProbabilityAdjust ? Math.Pow(0.1, DifficultyCalculationUtils.ReverseLerp(skill, MID_SKILL_THRESHOLD, LOW_SKILL_THRESHOLD) * 4) : 1.0;
        private double skillAdjust1(double skill) => skill * (1 + 0.25 * DifficultyCalculationUtils.ReverseLerp(skill, MID_SKILL_THRESHOLD, LOW_SKILL_THRESHOLD));
        private double skillAdjust2(double skill) => skill * (1 - 0.155 * DifficultyCalculationUtils.ReverseLerp(skill, MID_SKILL_THRESHOLD, LOW_SKILL_THRESHOLD));
        private double skillAdjust3(double skill) => skill * (1 + 0.185 * DifficultyCalculationUtils.ReverseLerp(skill, MID_SKILL_THRESHOLD, LOW_SKILL_THRESHOLD));

        private double difficultyValueExact()
        {
            double maxDiff = Difficulties.Max();
            if (maxDiff <= 1e-10) return 0;

            const double lower_bound = 0;
            double upperBoundEstimate = 3.0 * maxDiff;

            double skill = RootFinding.FindRootExpand(
                skill => fcProbability(skill) - FcProbability * probabilityAdjust(skill),
                lower_bound,
                upperBoundEstimate,
                accuracy: 1e-4);

            return skill;

            double fcProbability(double s)
            {
                if (s <= 0) return 0;

                return Difficulties.Aggregate<double, double>(1, (current, d) => current * HitProbability(s, d));
            }
        }

        private double difficultyValueBinned()
        {
            double maxDiff = Difficulties.Max();
            if (maxDiff <= 1e-10) return 0;

            var bins = Bin.CreateBins(Difficulties, bin_count);

            const double lower_bound = 0;
            double upperBoundEstimate = 3.0 * maxDiff;

            double skill = RootFinding.FindRootExpand(
                skill => fcProbability(skill) - FcProbability * probabilityAdjust(skill),
                lower_bound,
                upperBoundEstimate,
                accuracy: 1e-4);

            return skill;

            double fcProbability(double s)
            {
                if (s <= 0) return 0;

                return bins.Aggregate(1.0, (current, bin) => current * Math.Pow(HitProbability(s, bin.Difficulty), bin.Count));
            }
        }

        private double difficultyValue()
        {
            if (Difficulties.Count == 0) return 0;

            double skill = Difficulties.Count > binThreshold ? difficultyValueBinned() : difficultyValueExact();

            // Use adjusts BEFORE miss penalty calc to make miss penalty harsher for easy and long maps (even tho it would not be mathematically correct)
            if (UseProbabilityAdjust) skill = skillAdjust1(skill);
            if (UseDefaultMissProb) skill = skillAdjust2(skill);
            if (UseDifficultyPower) skill = skillAdjust3(skill);

            return skill;
        }

        public override double DifficultyValue()
        {
            double skill = difficultyValue();

            // This adjust is just global balancing to reduce nerf on lower end
            // Apply AFTER miss penalty to not mess things up
            //skill *= 1 + 0.12 * DifficultyCalculationUtils.ReverseLerp(skill, 4000, 2000);

            return skill;
        }

        public double StrainDifficultyValue() => base.DifficultyValue();

        /// <returns>
        /// A polynomial fitted to the miss counts at each skill level.
        /// </returns>
        public ExpPolynomial GetMissPenaltyCurve()
        {
            double[] missCounts = new double[7];
            double[] penalties = { 1, 0.95, 0.9, 0.8, 0.6, 0.3, 0 };

            ExpPolynomial missPenaltyCurve = new ExpPolynomial();

            // If there are no notes, we just return the curve with all coefficients set to zero.
            if (Difficulties.Count == 0 || Difficulties.Max() == 0)
                return missPenaltyCurve;

            double fcSkill = difficultyValue();

            var bins = Bin.CreateBins(Difficulties, bin_count);

            for (int i = 0; i < penalties.Length; i++)
            {
                if (i == 0)
                {
                    missCounts[i] = 0;
                    continue;
                }

                double penalizedSkill = fcSkill * penalties[i];

                missCounts[i] = getMissCountAtSkill(penalizedSkill, bins);
            }

            missPenaltyCurve.Fit(missCounts);

            return missPenaltyCurve;
        }

        /// <summary>
        /// Find the lowest miss count that a player with the provided <paramref name="skill"/> would have a 2% chance of achieving or better.
        /// </summary>
        private double getMissCountAtSkill(double skill, List<Bin> bins)
        {
            double maxDiff = Difficulties.Max();

            if (maxDiff == 0)
                return 0;
            if (skill <= 0)
                return Difficulties.Count;

            var poiBin = Difficulties.Count > binThreshold ? new PoissonBinomial(bins, skill, HitProbability) : new PoissonBinomial(Difficulties, skill, HitProbability);

            return Math.Max(0, RootFinding.FindRootExpand(x => poiBin.CDF(x) - FcProbability, -50, 1000, accuracy: 1e-4));
        }
    }
}
