// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuProbSkill : Skill
    {
        protected OsuProbSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// The skill level returned from this class will have FcProbability chance of hitting every note correctly.
        /// A higher value rewards short, high difficulty sections, whereas a lower value rewards consistent, lower difficulty.
        protected abstract double FcProbability { get; }

        private readonly List<double> difficulties = new List<double>();

        /// <summary>
        /// Returns the strain value at <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
        protected abstract double StrainValueAt(DifficultyHitObject current);

        public override void Process(DifficultyHitObject current)
        {
            difficulties.Add(StrainValueAt(current));
        }

        protected abstract double HitProbability(double skill, double difficulty);

        private double difficultyValueBinned()
        {
            double maxDiff = difficulties.Max();
            if (maxDiff <= 1e-10) return 0;

            var bins = Bin.CreateBins(difficulties);

            const double lower_bound = 0;
            double upperBoundEstimate = 3.0 * maxDiff;

            double skill = RootFinding.FindRootExpand(
                skill => fcProbability(skill) - FcProbability,
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

        private double difficultyValueExact()
        {
            double maxDiff = difficulties.Max();
            if (maxDiff <= 1e-10) return 0;

            const double lower_bound = 0;
            double upperBoundEstimate = 3.0 * maxDiff;

            double skill = RootFinding.FindRootExpand(
                skill => fcProbability(skill) - FcProbability,
                lower_bound,
                upperBoundEstimate,
                accuracy: 1e-4);

            return skill;

            double fcProbability(double s)
            {
                if (s <= 0) return 0;

                return difficulties.Aggregate<double, double>(1, (current, d) => current * HitProbability(s, d));
            }
        }

        public override double DifficultyValue()
        {
            if (difficulties.Count == 0)
                return 0;

            return difficulties.Count < 64 ? difficultyValueExact() : difficultyValueBinned();
        }

        /// <summary>
        /// Find the lowest misscount that a player with the provided <paramref name="skill"/> would have a 2% chance of achieving.
        /// </summary>
        public double GetMissCountAtSkill(double skill)
        {
            double maxDiff = difficulties.Max();

            if (maxDiff == 0)
                return 0;
            if (skill <= 0)
                return difficulties.Count;

            PoissonBinomial poiBin;

            if (difficulties.Count > 64)
            {
                var bins = Bin.CreateBins(difficulties);
                poiBin = new PoissonBinomial(bins, skill, HitProbability);
            }
            else
            {
                poiBin = new PoissonBinomial(difficulties, skill, HitProbability);
            }

            return Math.Max(0, RootFinding.FindRootExpand(x => poiBin.CDF(x) - FcProbability, -50, 1000, accuracy: 1e-4));
        }
    }
}
