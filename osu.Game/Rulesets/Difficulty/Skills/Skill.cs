// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class Skill
    {
        /// <summary>
        /// The peak strain for each <see cref="DifficultyCalculator.SectionLength"/> section of the beatmap.
        /// </summary>
        public IList<double> StrainPeaks => strainPeaks;

        private const int difficulty_count = 20;

        private const double max_strain_time = 200;
        private const double difficulty_multiplier = 0.0675;

        // repeating a section adds this much difficulty
        private const double star_bonus_per_length_double = 0.0655;
        private readonly double star_bonus_k = Math.Log(2) / star_bonus_per_length_double;

        // Constant difficulty sections of this length match previous star rating
        private const double star_bonus_base_time = (7.5 * 1000.0);

        // Final star rating is player skill level who can FC the map once per target_fc_time
        private const double target_fc_time = 4 * 60 * 60 * 1000;

        // minimum precision for fc time, though typically will be around 5x more precise
        // current setting of 0.05 usually takes 2 iterations, gives around 4dp for star ratings
        private const double target_fc_precision = 0.05;

        // maps with this expected length will match previous star rating
        private const double target_fc_base_time = 30 * 1000;

        private double target_fc_difficulty_adjustment => -skillLevel(target_fc_base_time / target_fc_time, 0);



        /// <summary>
        /// Strain values are multiplied by this number for the given skill. Used to balance the value of different skills between each other.
        /// </summary>
        protected abstract double SkillMultiplier { get; }

        /// <summary>
        /// Determines how quickly strain decays for the given skill.
        /// For example a value of 0.15 indicates that strain decays to 15% of its original value in one second.
        /// </summary>
        protected abstract double StrainDecayBase { get; }


        /// <summary>
        /// The weight by which each strain value decays.
        /// </summary>
        protected virtual double DecayWeight => 0.9; //TODO fix other game modes and remove

        /// <summary>
        /// <see cref="DifficultyHitObject"/>s that were processed previously. They can affect the strain values of the following objects.
        /// </summary>
        protected readonly LimitedCapacityStack<DifficultyHitObject> Previous = new LimitedCapacityStack<DifficultyHitObject>(2); // Contained objects not used yet

        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.
        private double currentSectionPeak = 1; // We also keep track of the peak strain level in the current section.

        private readonly List<double> strainPeaks = new List<double>(); // TODO remove and fix other modes

        private readonly List<double> expDifficulties = new List<double>(); // list of exp(k*difficulty) for each note
        private readonly List<double> timestamps = new List<double>();  // list of timestamps for each note

        private IList<double> missCountBySR, comboSR;
        private double fcProb=0;
        private double difficulty=0;

        public const double MissSRIncrement = 0.1;
        public IList<double> MissCounts { get => missCountBySR; }
        public IList<double> ComboSR { get => comboSR; }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values.
        /// Also calculates hit probability function for this note and adds to list
        /// </summary>
        public void Process(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += StrainValueOf(current) * SkillMultiplier;


            double legacyScalingFactor = 10;
            double stars = Math.Sqrt(currentStrain * legacyScalingFactor) * difficulty_multiplier;

            double t = Math.Min(current.DeltaTime, max_strain_time) / star_bonus_base_time;

            double expDifficulty = Math.Exp(star_bonus_k * starsToDifficulty(stars)) * t;



            expDifficulties.Add(expDifficulty);
            timestamps.Add(current.StartTime);


            Previous.Push(current);
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        public void SaveCurrentPeak() // TODO remove and fix other modes
        {
            if (Previous.Count > 0)
                strainPeaks.Add(currentSectionPeak);
        }

        /// <summary>
        /// Sets the initial strain level for a new section.
        /// </summary>
        /// <param name="offset">The beginning of the new section in milliseconds.</param>
        public void StartNewSectionFrom(double offset)  // TODO remove and fix other modes
        {
            // The maximum strain of the new section is not zero by default, strain decays as usual regardless of section boundaries.
            // This means we need to capture the strain level at the beginning of the new section, and use that as the initial peak level.
            if (Previous.Count > 0)
                currentSectionPeak = currentStrain * strainDecay(offset - Previous[0].BaseObject.StartTime);
        }

        /// <summary>
        /// Perform difficulty calculations
        /// </summary>
        public void Calculate()
        {

            // difficulty to FC the remainder of the map from every position, used later to calculate expected map length. 
            var difficultyPartialSums = new double[expDifficulties.Count];

            double total=0;

            // Difficulty calculated according to probability of FC
            // iterate backwards to calculate probs of FCing the remainder of map from a given point
            for (int i = expDifficulties.Count - 1; i >= 0; --i)
            {
                total += expDifficulties[i];
                difficultyPartialSums[i] = total;
            }

            difficulty = Math.Log(total) / star_bonus_k;

            calculateSkillToFcSubsets(difficultyPartialSums);

            for (int i=0; i< comboSR.Count; i++)
            {
                comboSR[i] = difficultyToStars(comboSR[i]);
            }

            calculateMissSR();

        }

        /// <summary>
        /// Returns the calculated difficulty to FC, representing all processed <see cref="DifficultyHitObject"/>s.
        /// </summary>
        public double DifficultyValue()
        {
            return comboSR.Last();
        }


        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected abstract double StrainValueOf(DifficultyHitObject current);

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        // get skill to fc easiest section with e.g. 5% combo, 10%, 15%, ... 100% combo
        private void calculateSkillToFcSubsets(double[] difficultyPartialSums)
        {
            comboSR = new double[difficulty_count];

            for (int i=1; i<=difficulty_count; ++i)
            {
                // getting lowest difficulty for a combo with this many hit objects
                int count = difficultyPartialSums.Length * i / difficulty_count;

                comboSR[i-1] = double.PositiveInfinity;

                for (int j=0;j<=difficulty_count-i; ++j)
                {
                    // checking this start point to see if it's easiest
                    int start = difficultyPartialSums.Length * j / difficulty_count;

                    // difficulty for the rest of the map after the combo we're considering
                    double remainder = (start+count<difficultyPartialSums.Length) ? difficultyPartialSums[start+count] : 0;

                    comboSR[i-1] = Math.Min(comboSR[i-1], getSkillToFcInTargetTime(difficultyPartialSums,start,count, remainder));
                }
            }
        }

        private double getSkillToFcInTargetTime(double[] difficultyPartialSums, int first, int count, double remainder)
        {
            int last = first + count - 1;

            if (difficultyPartialSums[first] - remainder <= 1e-10)
            {
                // remainder is the expDifficulty of the rest of the map after last
                // if first and remainder are equal, the section has no notes, so return zero stars
                return -1e100;
            }

            double difficulty = Math.Log(difficultyPartialSums[first]-remainder) / star_bonus_k;

            // hard to calculate skill directly so approximate and iteratively improve. Normally gets to within 1% in 2 iterations

            // initial guess of average play length 
            double averageLength = (timestamps[last]-timestamps[first]) * 0.3;

            // fcProb being a member variable is horrible. Want to use fcProb for whole map in miss SR calc
            // the last time this function is called happens to be for the whole map
            fcProb = averageLength / target_fc_time;

            // map is super long, these calculations might not even make sense, return skill level to pass with 50% probability
            if (fcProb > 0.9)
            {
                fcProb = 0.5;
                return skillLevel(0.5, difficulty) + target_fc_difficulty_adjustment;
            }


            double skill = skillLevel(fcProb, difficulty);
            double firstLength;

            int max_iterations = 5;

            for (int i = 0; i < max_iterations; ++i)
            {
                // use estimate for improved average length calculation
                double expectedTimeBeforeFC = getExpectedTimePlayedBeforeFC(skill, difficultyPartialSums, first, count, remainder);

                // x ms per fc, fc with probability p per attempt, so on average x*p ms per attempt
                averageLength = expectedTimeBeforeFC * passProbability(skill, difficulty);
                fcProb = averageLength / target_fc_time;

                // map too long
                if (fcProb > 0.9)
                {
                    fcProb = 0.5;
                    return skillLevel(0.5, difficulty) + target_fc_difficulty_adjustment;
                }

                skill = skillLevel(fcProb, difficulty);

                if (Math.Abs(expectedTimeBeforeFC - target_fc_time) / target_fc_time < target_fc_precision)
                {
                    // enough precision already
                    break;
                }
            }

            if (fcProb > 0.5)
            {
                // map is very long, return skill level to pass with 50% probability
                fcProb = 0.5;
                return skillLevel(0.5, difficulty) + target_fc_difficulty_adjustment;
            }

            return skill + target_fc_difficulty_adjustment;
        }

        private double getExpectedTimePlayedBeforeFC(double skill, double[] difficultyPartialSums, int first, int count, double remainder = 0)
        {
            int last = first + count - 1;

            // note: calculating this separately for each skill isn't really correct, maybe fix in future

            double length = 0;
            double lastTime = timestamps[first] - max_strain_time;
            double expSkill = Math.Exp(-star_bonus_k * skill);

            for (int i=first; i<=last; ++i)
            {
                if (i < last)
                {
                    // no need for millisecond precision, 1s fine
                    if ((timestamps[i + 1] - lastTime) < 1000)
                        continue;
                }

                double deltaT = timestamps[i] - lastTime;

                // We need to play this note again every time we fail after it until we pass the remainder of the map
                // If we fail before, we don't play it

                // difficultyPartialSums is a list of exp(k*d) for each note where d is the difficulty to pass the remainder of the map
                double probabilityOfPassingRemainderOfMap = passProbFromExp(expSkill, difficultyPartialSums[i]-remainder);
                double expectedNotePlaysBeforeFC = 1 / probabilityOfPassingRemainderOfMap;
                length += expectedNotePlaysBeforeFC*(deltaT);
                lastTime = timestamps[i];
            }

            return length;
        }

        // probability a player of the given skill passes a map of the given difficulty
        private double passProbability(double skill, double difficulty) => Math.Exp(-Math.Exp(-star_bonus_k * (skill - difficulty)));

        // same as above but pass in exp(-k*skill) and exp(k*difficulty)
        private double passProbFromExp(double expSkill, double expDifficulty) => Math.Exp(-expSkill * expDifficulty);

        // inverse of passProbability
        private double skillLevel(double probability, double difficulty) => difficulty - Math.Log(-Math.Log(probability)) / star_bonus_k;
 
        private double starsToDifficulty(double val)
        {
            if (val > 0)
                return (Math.Log(val));

            return -1e100;
        }

        private double difficultyToStars(double val)
        {
            return Math.Exp(val);
        }


        private void calculateMissSR()
        {
            missCountBySR = new double[difficulty_count];

            double stars = comboSR.Last();
            for (int i = 0; i < difficulty_count; ++i)
            {
                double missStars = stars - (i+1) * MissSRIncrement;

                // skill is the same skill who can FC a missStars map with same length as this one in 4 hours
                double skill = starsToDifficulty(missStars)  - target_fc_difficulty_adjustment;

                double[] missProbs = getMissProbabilities(skill);

                missCountBySR[i] = getMissCount(fcProb, missProbs);
            }
        }

        private double[] getMissProbabilities(double skill)
        {
            // slider breaks should be a miss :(

            var result = new double[expDifficulties.Count];

            double expSkill = Math.Exp(-star_bonus_k * skill);

            for (int i = 0; i < expDifficulties.Count; ++i)
            {
                result[i] = 1-passProbFromExp(expSkill, expDifficulties[i]);
            }

            return result;
        }

        // find first miss count achievable with at least probability p
        private int getMissCount(double p, double[] missProbabilities)
        {
            var distribution = new PoissionBinomial(missProbabilities);

            int missCount = 0;
            while (missCount < 10000)
            {
                if (distribution.CDF(missCount) > p)
                {
                    return missCount;
                }

                ++missCount;
            }
            return 10000;
        }

        // note: NOT poisson OR binomial, it's its own thing
        private class PoissionBinomial
        {

            // approximate poisson binomial CDF defined by miss probabilities
            // see "Refined Normal Approximation (RNA)" from
            // https://www.researchgate.net/publication/257017356_On_computing_the_distribution_function_for_the_Poisson_binomial_distribution
            // In future maybe use an exact method for small map length


            double mu, sigma, gamma, v;

            public PoissionBinomial(IList<double> probabilities)
            {
                mu = probabilities.Sum();

                sigma = 0;
                gamma = 0;

                foreach (double p in probabilities)
                {
                    sigma += p * (1 - p);
                    gamma += p * (1 - p) * (1 - 2 * p);
                }

                sigma = Math.Sqrt(sigma);

                v = gamma / (6 * Math.Pow(sigma, 3));
            }

            public double CDF(double count)
            {
                double k = (count + 0.5 - mu) / sigma;

                double result = Normal.CDF(0, 1, k) + v * (1 - k * k) * Normal.PDF(0, 1, k);
                if (result < 0) return 0;
                if (result > 1) return 1;
                return result;
            }
        }
    }
}
