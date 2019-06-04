// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#define OSU_SKILL_STRAIN_AFTER_NOTE // defined => use time between current and next, commented => use time between prev and current

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class OsuSkill : Skill
    {
        protected virtual double MaxStrainTime => 200;
        private const double difficulty_multiplier = 0.0675;

        // repeating a section adds this much difficulty
        protected virtual double StarBonusPerLengthDouble => 0.0655;
        private double starBonusK => Math.Log(2) / StarBonusPerLengthDouble;

        // Constant difficulty sections of this length match previous star rating
        protected virtual double StarBonusBaseTime => (8.0 * 1000.0);

        // Final star rating is player skill level who can FC the map once per target_fc_time
        private const double target_fc_time = 4 * 60 * 60 * 1000;

        // minimum precision for fc time, though typically will be around 5x more precise
        // current setting of 0.05 usually takes 2 iterations, gives around 4dp for star ratings
        private const double target_fc_precision = 0.05;

        // maps with this expected length will match previous star rating
        private const double target_fc_base_time = 30 * 1000;

        private double targetFcDifficultyAdjustment => -skillLevel(target_fc_base_time / target_fc_time, 0);

        // size of lists used to interpolate combo SR and miss count SR for performance calc
        private const int difficulty_count = 20;

        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.

        private readonly List<double> expDifficulties = new List<double>(); // list of exp(k*difficulty) for each note
        private readonly List<double> timestamps = new List<double>(); // list of timestamps for each note
        private double fcProb;
        private bool lastScaled;

        public const double MISS_STAR_RATING_INCREMENT = 0.1;
        public IList<double> MissCounts { get; private set; }
        public IList<double> ComboStarRatings { get; private set; }

        public IList<double> Timestamps => timestamps;

        public IEnumerable<double> HitObjectStars() => expDifficulties.Select(d => difficultyToStars(Math.Log(d) / starBonusK));

        public IEnumerable<double> CumulativeHitObjectStars()
        {
            double total = 0;

            // Difficulty calculated according to probability of FC
            // iterate backwards to calculate probs of FCing the remainder of map from a given point
            foreach (double d in expDifficulties)
            {
                total += d;
                yield return difficultyToStars(Math.Log(total) / starBonusK);
            }
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values.
        /// Also calculates hit probability function for this note and adds to list
        /// </summary>
        public override void Process(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += StrainValueOf(current) * SkillMultiplier;

            const double legacy_scaling_factor = 10;
            double stars = Math.Sqrt(currentStrain * legacy_scaling_factor) * difficulty_multiplier;

#if OSU_SKILL_STRAIN_AFTER_NOTE
            scaleLastHitObject(current.DeltaTime);
#endif

            double expDifficulty = Math.Exp(starBonusK * starsToDifficulty(stars));

            // add zero difficulty notes corresponding to slider ticks/slider ends so combo is reflected properly
            // (slider difficulty is currently handled in the following note)
            int extraNestedCount = current.BaseObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; ++i)
            {
                expDifficulties.Add(0);
                timestamps.Add(current.StartTime);
            }

            expDifficulties.Add(expDifficulty);
            timestamps.Add(current.StartTime);

#if !OSU_SKILL_STRAIN_AFTER_NOTE
            scaleLastHitObject(current.DeltaTime);
#endif

            Previous.Push(current);
        }

        private void scaleLastHitObject(double t = double.PositiveInfinity)
        {
            if (expDifficulties.Count != 0)
                expDifficulties[expDifficulties.Count - 1] *= Math.Min(t, MaxStrainTime) / StarBonusBaseTime;
        }

        /// <summary>
        /// Perform difficulty calculations
        /// </summary>
        public override void Calculate()
        {
#if OSU_SKILL_STRAIN_AFTER_NOTE
            if (!lastScaled)
            {
                scaleLastHitObject();
                lastScaled = true;
            }
#endif

            // difficulty to FC the remainder of the map from every position, used later to calculate expected map length.
            var difficultyPartialSums = new double[expDifficulties.Count];

            double total = 0;

            // Difficulty calculated according to probability of FC
            // iterate backwards to calculate probs of FCing the remainder of map from a given point
            for (int i = expDifficulties.Count - 1; i >= 0; --i)
            {
                total += expDifficulties[i];
                difficultyPartialSums[i] = total;
            }

            //double difficulty = Math.Log(total) / starBonusK;

            calculateSkillToFcSubsets(difficultyPartialSums);

            for (int i = 0; i < ComboStarRatings.Count; i++)
            {
                ComboStarRatings[i] = difficultyToStars(ComboStarRatings[i]);
            }

            calculateMissStarRating();
        }

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        // get skill to fc easiest section with e.g. 5% combo, 10%, 15%, ... 100% combo
        private void calculateSkillToFcSubsets(double[] difficultyPartialSums)
        {
            ComboStarRatings = new double[difficulty_count];

            for (int i = 1; i <= difficulty_count; ++i)
            {
                // getting lowest difficulty for a combo with this many hit objects
                int count = difficultyPartialSums.Length * i / difficulty_count;

                ComboStarRatings[i - 1] = double.PositiveInfinity;

                for (int j = 0; j <= difficulty_count - i; ++j)
                {
                    // checking this start point to see if it's easiest
                    int start = difficultyPartialSums.Length * j / difficulty_count;

                    // difficulty for the rest of the map after the combo we're considering
                    double remainder = (start + count < difficultyPartialSums.Length) ? difficultyPartialSums[start + count] : 0;

                    ComboStarRatings[i - 1] = Math.Min(ComboStarRatings[i - 1], getSkillToFcInTargetTime(difficultyPartialSums, start, count, remainder));
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

            double difficulty = Math.Log(difficultyPartialSums[first] - remainder) / starBonusK;

            // hard to calculate skill directly so approximate and iteratively improve. Normally gets to within 1% in 2 iterations

            // initial guess of average play length
            double averageLength = (timestamps[last] - timestamps[first]) * 0.3;

            // fcProb being a member variable is horrible. Want to use fcProb for whole map in miss SR calc
            // the last time this function is called happens to be for the whole map
            fcProb = averageLength / target_fc_time;

            // map is super long, these calculations might not even make sense, return skill level to pass with 50% probability
            if (fcProb > 0.9)
            {
                fcProb = 0.5;
                return skillLevel(0.5, difficulty) + targetFcDifficultyAdjustment;
            }

            double skill = skillLevel(fcProb, difficulty);

            const int max_iterations = 5;

            for (int i = 0; i < max_iterations; ++i)
            {
                // use estimate for improved average length calculation
                double expectedTimeBeforeFc = getExpectedTimePlayedBeforeFc(skill, difficultyPartialSums, first, count, remainder);

                // x ms per fc, fc with probability p per attempt, so on average x*p ms per attempt
                averageLength = expectedTimeBeforeFc * passProbability(skill, difficulty);
                fcProb = averageLength / target_fc_time;

                // map too long
                if (fcProb > 0.9)
                {
                    fcProb = 0.5;
                    return skillLevel(0.5, difficulty) + targetFcDifficultyAdjustment;
                }

                skill = skillLevel(fcProb, difficulty);

                if (Math.Abs(expectedTimeBeforeFc - target_fc_time) / target_fc_time < target_fc_precision)
                {
                    // enough precision already
                    break;
                }
            }

            if (fcProb > 0.5)
            {
                // map is very long, return skill level to pass with 50% probability
                fcProb = 0.5;
                return skillLevel(0.5, difficulty) + targetFcDifficultyAdjustment;
            }

            return skill + targetFcDifficultyAdjustment;
        }

        private double getExpectedTimePlayedBeforeFc(double skill, double[] difficultyPartialSums, int first, int count, double remainder = 0)
        {
            int last = first + count - 1;

            // note: calculating this separately for each skill isn't really correct, maybe fix in future

            double length = 0;
            double lastTime = timestamps[first] - MaxStrainTime;
            double expSkill = Math.Exp(-starBonusK * skill);

            for (int i = first; i <= last; ++i)
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
                double probabilityOfPassingRemainderOfMap = passProbFromExp(expSkill, difficultyPartialSums[i] - remainder);
                double expectedNotePlaysBeforeFc = 1 / probabilityOfPassingRemainderOfMap;
                length += expectedNotePlaysBeforeFc * (deltaT);
                lastTime = timestamps[i];
            }

            return length;
        }

        // probability a player of the given skill passes a map of the given difficulty
        private double passProbability(double skill, double difficulty) => Math.Exp(-Math.Exp(-starBonusK * (skill - difficulty)));

        // same as above but pass in exp(-k*skill) and exp(k*difficulty)
        private double passProbFromExp(double expSkill, double expDifficulty) => Math.Exp(-expSkill * expDifficulty);

        // inverse of passProbability
        private double skillLevel(double probability, double difficulty) => difficulty - Math.Log(-Math.Log(probability)) / starBonusK;

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

        private void calculateMissStarRating()
        {
            MissCounts = new double[difficulty_count];

            double stars = ComboStarRatings.Last();

            for (int i = 0; i < difficulty_count; ++i)
            {
                double missStars = stars - (i + 1) * MISS_STAR_RATING_INCREMENT;

                // skill is the same skill who can FC a missStars map with same length as this one in 4 hours
                double skill = starsToDifficulty(missStars) - targetFcDifficultyAdjustment;

                double[] missProbs = getMissProbabilities(skill);

                MissCounts[i] = getMissCount(fcProb, missProbs);
            }
        }

        private double[] getMissProbabilities(double skill)
        {
            // slider breaks should be a miss :(

            var result = new double[expDifficulties.Count];

            double expSkill = Math.Exp(-starBonusK * skill);

            for (int i = 0; i < expDifficulties.Count; ++i)
            {
                result[i] = 1 - passProbFromExp(expSkill, expDifficulties[i]);
            }

            return result;
        }

        // find first miss count achievable with at least probability p
        private List<double> printMissDistribution(double[] missProbabilities)
        {
            var distribution = new PoissionBinomial(missProbabilities);
            var result = new List<double>();
            int missCount = 0;

            while (missCount < 10000)
            {
                double p = distribution.Cdf(missCount);
                result.Add(p);

                if (p > 0.99)
                {
                    break;
                }

                ++missCount;
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
                if (distribution.Cdf(missCount) > p)
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

            private readonly double mu, sigma, v;

            public PoissionBinomial(IList<double> probabilities)
            {
                mu = probabilities.Sum();

                sigma = 0;
                double gamma = 0;

                foreach (double p in probabilities)
                {
                    sigma += p * (1 - p);
                    gamma += p * (1 - p) * (1 - 2 * p);
                }

                sigma = Math.Sqrt(sigma);

                v = gamma / (6 * Math.Pow(sigma, 3));
            }

            public double Cdf(double count)
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
