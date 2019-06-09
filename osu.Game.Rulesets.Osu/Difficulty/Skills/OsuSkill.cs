// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// If OSU_SKILL_STRAIN_AFTER_NOTE defined, a note's duration is the time between current note and next note,
// If commented, it's the duration between previous and current.

#define OSU_SKILL_STRAIN_AFTER_NOTE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class OsuSkill : Skill
    {
        /// <summary>
        /// Strain time is assigned to a note for the minimum of it's duration and this value.
        /// </summary>
        protected virtual double MaxStrainTime => 200;

        /// <summary>
        /// Star multiplier from legacy difficulty calc
        /// </summary>
        private const double difficulty_multiplier = 0.0675;

        /// <summary>
        /// Repeating a section multiplies difficulty by this factor
        /// Increasing this number increases the impact of map length on SR and decreases the impact of difficulty spikes.
        /// </summary>
        protected virtual double StarMultiplierPerRepeat => 1.0677;

        private double starBonusK => 1 / Math.Log(StarMultiplierPerRepeat, 2);

        /// <summary>
        /// Constant difficulty sections of this length match old difficulty values.
        /// Decreasing this value increases star rating of all maps equally
        /// </summary>
        protected virtual double StarBonusBaseTime => (8.0 * 1000.0);

        /// <summary>
        /// Final star rating is player skill level who can FC the map once per this amount of time (in ms).
        /// Decrease this number to give longer maps more PP.
        /// </summary>
        private const double target_retry_time_before_fc = 3.5 * 60 * 60 * 1000;

        /// <summary>
        /// Minimum precision for time spent for a player to full combo the map, though typically will be around 5x more precise.
        /// </summary>
        private const double target_fc_precision = 0.05; // current setting of 0.05 usually takes 2 iterations, gives around 4dp for star ratings

        /// <summary>
        /// Maps with this expected length will match legacy PP values.
        /// Decrease this value to increase PP For all maps equally
        /// </summary>
        private const double target_fc_base_time = 33 * 1000;

        /// <summary>
        /// Time taken to retry and get to the beginning of a map.
        /// Increasing adds more weight to the first few notes of a map when calculating expected time to FC.
        /// </summary>
        private const double minimum_retry_time = 3 * 1000;

        /// <summary>
        /// Multiplier used to preserve star rating for maps with length <see cref="target_fc_base_time"/>
        /// </summary>
        private double targetFcDifficultyMultiplier => 1 / skillLevel(target_fc_base_time / target_retry_time_before_fc, 1);

        /// <summary>
        /// Size of lists used to interpolate combo difficulty value and miss count difficulty value for performance calculations.
        /// </summary>
        private const int difficulty_count = 20;

        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.

        private readonly List<double> powDifficulties = new List<double>(); // list of difficulty^k for each note
        private readonly List<double> timestamps = new List<double>(); // list of timestamps for each note
        private double fcProb;
        private bool lastScaled;

        public const double MISS_STAR_RATING_INCREMENT = 0.1;
        public IList<double> MissCounts { get; private set; }
        public IList<double> ComboStarRatings { get; private set; }

        public IList<double> Timestamps => timestamps;

        public double Difficulty { get; private set; }

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

            double powDifficulty = Math.Pow(stars, starBonusK);

            // add zero difficulty notes corresponding to slider ticks/slider ends so combo is reflected properly
            // (slider difficulty is currently handled in the following note)
            int extraNestedCount = current.BaseObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; ++i)
            {
                powDifficulties.Add(0);
                timestamps.Add(current.StartTime);
            }

            powDifficulties.Add(powDifficulty);
            timestamps.Add(current.StartTime);

#if !OSU_SKILL_STRAIN_AFTER_NOTE
            scaleLastHitObject(current.DeltaTime);
#endif

            Previous.Push(current);
        }

        /// <summary>
        /// Apply note duration scaling to the last hit object
        /// </summary>
        private void scaleLastHitObject(double t = double.PositiveInfinity)
        {
            if (powDifficulties.Count != 0)
                powDifficulties[powDifficulties.Count - 1] *= Math.Min(t, MaxStrainTime) / StarBonusBaseTime;
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
            var difficultyPartialSums = new double[powDifficulties.Count];

            double total = 0;

            // Difficulty calculated according to probability of FC
            // iterate backwards to calculate probs of FCing the remainder of map from a given point
            for (int i = powDifficulties.Count - 1; i >= 0; --i)
            {
                total += powDifficulties[i];
                difficultyPartialSums[i] = total;
            }

            Difficulty = Math.Pow(total, 1 / starBonusK);

            calculateSkillToFcSubsets(difficultyPartialSums);

            for (int i = 0; i < ComboStarRatings.Count; i++)
            {
                ComboStarRatings[i] = difficultyToStars(ComboStarRatings[i]);
            }

            calculateMissStarRating();
        }

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        /// <summary>
        /// Get skill to fc easiest section with e.g. 5% combo, 10%, 15%, ... 100% combo.
        /// </summary>
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

        /// <summary>
        /// Calculates skill required to Full Combo the given section of a map in target_fc_time.
        /// </summary>
        private double getSkillToFcInTargetTime(double[] difficultyPartialSums, int first, int count, double remainder)
        {
            int last = first + count - 1;

            if (Math.Abs(difficultyPartialSums[first] - remainder) <= 1e-10)
            {
                // remainder is the powDifficulty of the rest of the map after "last"
                // if first and remainder are equal, the section has no notes, so return zero stars
                return 0;
            }

            double difficulty = Math.Pow(difficultyPartialSums[first] - remainder, 1 / starBonusK);
            double mapLength = timestamps[last] - timestamps[first];
            double targetFcTime = target_retry_time_before_fc + mapLength;

            // hard to calculate skill directly so approximate and iteratively improve. Normally gets to within 1% in 2 iterations

            // initial guess of average play length
            double averageLength = mapLength * 0.3;

            // fcProb being a member variable is horrible. Want to use fcProb for whole map in miss SR calc
            // the last time this function is called happens to be for the whole map
            fcProb = averageLength / targetFcTime;

            double skill = skillLevel(fcProb, difficulty);

            const int max_iterations = 5;

            for (int i = 0; i < max_iterations; ++i)
            {
                // use estimate for improved average length calculation
                double expectedTimeBeforeFc = getExpectedTimePlayedBeforeFc(skill, difficultyPartialSums, first, count, remainder);

                // x ms per fc, fc with probability p per attempt, so on average x*p ms per attempt
                averageLength = expectedTimeBeforeFc * passProbability(skill, difficulty);
                fcProb = averageLength / targetFcTime;

                skill = skillLevel(fcProb, difficulty);

                if (Math.Abs(expectedTimeBeforeFc - targetFcTime) / targetFcTime < target_fc_precision)
                {
                    // enough precision already
                    break;
                }
            }

            return skill * targetFcDifficultyMultiplier;
        }

        /// <summary>
        /// Calculate the average time a player with the given skill will take to Full Combo the given section of a map.
        /// </summary>
        private double getExpectedTimePlayedBeforeFc(double skill, double[] difficultyPartialSums, int first, int count, double remainder = 0)
        {
            int last = first + count - 1;

            // note: calculating this separately for each skill isn't really correct, maybe fix in future

            double length = 0;
            double lastTime = timestamps.First() - minimum_retry_time;
            double powSkill = Math.Pow(skill, -starBonusK);

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

                // difficultyPartialSums is a list of d^k for each note where d is the difficulty to pass the remainder of the map
                double probabilityOfPassingRemainderOfMap = passProbFromPow(powSkill, difficultyPartialSums[i] - remainder);
                double expectedNotePlaysBeforeFc = 1 / probabilityOfPassingRemainderOfMap;
                length += expectedNotePlaysBeforeFc * (deltaT);
                lastTime = timestamps[i];
            }

            return length;
        }

        /// <summary>
        /// The probability a player of the given skill passes a map of the given difficulty
        /// </summary>
        private double passProbability(double skill, double difficulty) => Math.Exp(-Math.Pow(skill / difficulty, -starBonusK));

        /// <summary>
        /// The probability a player of the given skill passes a map of the given difficulty. Unlike passProbability, this gives an exponential relationship with skill and difficulty.
        /// </summary>
        private double passProbFromPow(double powSkill, double powDifficulty) => Math.Exp(-powSkill * powDifficulty);

        /// <summary>
        /// Player skill level that passes a map of the given difficulty with the given probability
        /// </summary>
        private double skillLevel(double probability, double difficulty) => difficulty * Math.Pow(-Math.Log(probability), -1 / starBonusK);

        /// <summary>
        /// Convert star rating (calculated from strain) to "difficulty" in the probability formula.
        /// </summary>
        private double starsToDifficulty(double val)
        {
            return val;
        }

        /// <summary>
        /// Convert "difficulty" in the probability formula to star rating (calculated from strain).
        /// </summary>
        private double difficultyToStars(double val)
        {
            return val;
        }

        /// <summary>
        /// Calculate miss count for a list of star ratings (used to evaluate miss count of plays).
        /// </summary>
        private void calculateMissStarRating()
        {
            MissCounts = new double[difficulty_count];

            double stars = ComboStarRatings.Last();

            for (int i = 0; i < difficulty_count; ++i)
            {
                double missStars = stars - (i + 1) * MISS_STAR_RATING_INCREMENT;

                // skill is the same skill who can FC a missStars map with same length as this one in 4 hours
                double skill = starsToDifficulty(missStars) / targetFcDifficultyMultiplier;

                double[] missProbs = getMissProbabilities(skill);

                MissCounts[i] = getMissCount(fcProb, missProbs);
            }
        }

        /// <summary>
        /// Calculate the probability of missing each note given a skill level.
        /// </summary>
        private double[] getMissProbabilities(double skill)
        {
            // slider breaks should be a miss :(

            var result = new double[powDifficulties.Count];

            double powSkill = Math.Pow(skill, -starBonusK);

            for (int i = 0; i < powDifficulties.Count; ++i)
            {
                result[i] = 1 - passProbFromPow(powSkill, powDifficulties[i]);
            }

            return result;
        }

        /// <summary>
        /// Find first miss count achievable with at least probability p
        /// </summary>
        private int getMissCount(double p, double[] missProbabilities)
        {
            var distribution = new PoissonBinomial(missProbabilities);

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
    }
}
