// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class TimeSkill : Skill
    {
        protected TimeSkill(Mod[] mods)
            : base(mods)
        {
        }

        private const double ms_to_minutes = 1.0 / 60000.0;

        // FC time specific constants
        private const double time_threshold_minutes = 24;
        private const double max_delta_time = 5000;

        // Bin specific constants
        private const double bin_threshold_note_count = 64;
        private const int difficulty_bin_count = 8;
        private const int time_bin_count = 16;

        private const double epsilon = 1e-4;

        private readonly List<double> times = new List<double>();

        /// <summary>
        /// Returns the strain value at <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
        protected abstract double StrainValueAt(DifficultyHitObject current);

        protected override double ProcessInternal(DifficultyHitObject current)
        {
            times.Add(times.LastOrDefault() + Math.Min(current.DeltaTime, max_delta_time));

            return StrainValueAt(current);
        }

        protected abstract double HitProbability(double skill, double difficulty);

        public override double DifficultyValue()
        {
            if (ObjectDifficulties.Count == 0 || ObjectDifficulties.Max() <= epsilon)
                return 0;

            // We only initialize bins if we have enough notes to use them.
            List<Bin>? binList = null;

            if (ObjectDifficulties.Count > bin_threshold_note_count)
            {
                binList = Bin.CreateBins(ObjectDifficulties, times, difficulty_bin_count, time_bin_count);
            }

            // Lower bound and upper bound are generally unimportant
            return RootFinding.FindRootExpand(skill => timeSpentRetryingAtSkill(skill, binList) - time_threshold_minutes, 0, 10);
        }

        private double timeSpentRetryingAtSkill(double skill, List<Bin>? binList = null)
        {
            if (skill <= 0) return double.PositiveInfinity;

            double timeSpentRetrying = 0;
            double hitProbabilityProduct = 1;

            // We use bins, falling back to exact difficulty calculation if not available.
            if (binList is not null)
            {
                for (int timeIndex = time_bin_count - 1; timeIndex >= 0; timeIndex--)
                {
                    double deltaTime = times.LastOrDefault() / time_bin_count;

                    for (int difficultyIndex = 0; difficultyIndex < difficulty_bin_count; difficultyIndex++)
                    {
                        Bin bin = binList[difficulty_bin_count * timeIndex + difficultyIndex];

                        hitProbabilityProduct *= Math.Pow(HitProbability(skill, bin.Difficulty), bin.NoteCount);
                    }

                    timeSpentRetrying += deltaTime / hitProbabilityProduct - deltaTime;
                }
            }
            else
            {
                for (int n = ObjectDifficulties.Count - 1; n >= 0; n--)
                {
                    double deltaTime = n > 0 ? times[n] - times[n - 1] : times[n];

                    hitProbabilityProduct *= HitProbability(skill, ObjectDifficulties[n]);
                    timeSpentRetrying += deltaTime / hitProbabilityProduct - deltaTime;
                }
            }

            return timeSpentRetrying * ms_to_minutes;
        }

        /// <summary>
        /// The coefficients of a quartic fitted to the miss counts at each skill level.
        /// </summary>
        /// <returns>The coefficients for our penalty polynomial.</returns>
        public double[] GetMissPenaltyCoefficients()
        {
            Dictionary<double, double> missCounts = new Dictionary<double, double>();

            // If there are no notes, we just return a zero-polynomial.
            if (ObjectDifficulties.Count == 0 || ObjectDifficulties.Max() == 0)
                return Array.Empty<double>();

            double fcSkill = DifficultyValue();

            var bins = Bin.CreateBins(ObjectDifficulties, times, difficulty_bin_count, time_bin_count);

            foreach (double skillProportion in PolynomialPenaltyUtils.SKILL_PROPORTIONS)
            {
                if (skillProportion == 1)
                {
                    missCounts[skillProportion] = 0;
                    continue;
                }

                double penalizedSkill = fcSkill * skillProportion;

                // We take the log to squash miss counts, which have large absolute value differences, but low relative differences, into a straighter line for the polynomial.
                missCounts[skillProportion] = Math.Log(getMissCountAtSkill(penalizedSkill, bins) + 1);
            }

            return PolynomialPenaltyUtils.GetPenaltyCoefficients(missCounts);
        }

        /// <summary>
        /// Find the lowest misscount that a player with the provided <paramref name="skill"/> would likely achieve within 12 minutes of retrying.
        /// </summary>
        private double getMissCountAtSkill(double skill, List<Bin> bins)
        {
            double maxDiff = ObjectDifficulties.Max();
            double endTime = times.Max();

            if (maxDiff == 0)
                return 0;
            if (skill <= 0)
                return ObjectDifficulties.Count;

            IterativePoissonBinomial poiBin = new IterativePoissonBinomial();

            return Math.Max(0, RootFinding.FindRootExpand(x => retryTimeRequiredToObtainMissCount(x) - time_threshold_minutes, -50, 1000, accuracy: 0.01));

            double retryTimeRequiredToObtainMissCount(double missCount)
            {
                poiBin.Reset();

                double timeSpentRetrying = 0;

                if (ObjectDifficulties.Count > time_bin_count * difficulty_bin_count)
                {
                    double binTimeSteps = endTime / time_bin_count;

                    for (int timeIndex = 0; timeIndex < time_bin_count; timeIndex++)
                    {
                        for (int difficultyIndex = 0; difficultyIndex < difficulty_bin_count; difficultyIndex++)
                        {
                            Bin bin = bins[timeIndex * difficulty_bin_count + difficultyIndex];

                            double missProb = 1 - HitProbability(skill, bin.Difficulty);
                            poiBin.AddBinnedProbabilities(missProb, bin.NoteCount);
                        }

                        timeSpentRetrying += binTimeSteps * poiBin.Cdf(missCount);
                    }
                }
                else
                {
                    for (int i = 0; i < ObjectDifficulties.Count; i++)
                    {
                        double deltaTime = i > 0 ? times[i] - times[i - 1] : times[i];

                        double missProb = 1 - HitProbability(skill, ObjectDifficulties[i]);
                        poiBin.AddProbability(missProb);

                        timeSpentRetrying += deltaTime * poiBin.Cdf(missCount);
                    }
                }

                if (poiBin.Cdf(missCount) < 1e-10)
                    return double.PositiveInfinity;

                return (timeSpentRetrying / poiBin.Cdf(missCount) - endTime) * ms_to_minutes;
            }
        }

        /// <summary>
        /// Calculates the number of strains weighted against the top strain.
        /// The result is scaled by clock rate as it affects the total number of strains.
        /// </summary>
        public virtual double CountTopWeightedStrains(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            // What would the top strain be if all strain values were identical.
            // We don't have decay weight in FC time, so we just use the old live one of 0.95.
            double consistentTopStrain = difficultyValue * (1 - 0.95);

            if (consistentTopStrain == 0)
                return ObjectDifficulties.Count;

            // Use a weighted sum of all strains. Constants are arbitrary and give nice values
            return ObjectDifficulties.Sum(s => 1.1 / (1 + Math.Exp(-10 * (s / consistentTopStrain - 0.88))));
        }

        public static double DifficultyToPerformance(double difficulty) => 4.0 * Math.Pow(difficulty, 3.0);
    }
}
