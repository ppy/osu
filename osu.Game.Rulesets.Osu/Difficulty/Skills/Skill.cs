// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="OsuDifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class Skill
    {
        protected const double SINGLE_SPACING_THRESHOLD = 125;
        protected const double STREAM_SPACING_THRESHOLD = 110;

        private const double section_length = 400;
        private const double difficulty_multiplier = 0.0675;

        // repeating a section adds this much difficulty
        private const double star_bonus_per_length_double = 0.066;
        private readonly double star_bonus_k = Math.Log(2) / star_bonus_per_length_double;

        // Constant difficulty sections of this length match previous star rating
        private const double star_bonus_base_time = (12.0 * 1000.0);
        private const double time_multiplier = section_length / star_bonus_base_time;

        // Final star rating is player skill level who can FC the map once per target_fc_time
        private const double target_fc_time = 4 * 60 * 60 * 1000;
        private const double target_fc_sections = target_fc_time / section_length;

        // minimum precision for fc time, though typically will be around 5x more precise
        // current setting of 0.05 usually takes 2 iterations, gives around 4dp for star ratings
        private const double target_fc_precision = 0.05;

        // maps with this expected length will match previous star rating
        private const double target_fc_base_time = 30 * 1000;


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
        /// <see cref="OsuDifficultyHitObject"/>s that were processed previously. They can affect the strain values of the following objects.
        /// </summary>
        protected readonly History<OsuDifficultyHitObject> Previous = new History<OsuDifficultyHitObject>(2); // Contained objects not used yet

        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.
        private double currentSectionPeak = 1; // We also keep track of the peak strain level in the current section.
        private readonly List<double> strainPeaks = new List<double>();

        /// <summary>
        /// Process an <see cref="OsuDifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        public void Process(OsuDifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            if (!(current.BaseObject is Spinner))
                currentStrain += StrainValueOf(current) * SkillMultiplier;

            currentSectionPeak = Math.Max(currentStrain, currentSectionPeak);

            Previous.Push(current);
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        public void SaveCurrentPeak()
        {
            if (Previous.Count > 0)
                strainPeaks.Add(currentSectionPeak);
        }

        /// <summary>
        /// Sets the initial strain level for a new section.
        /// </summary>
        /// <param name="offset">The beginning of the new section in milliseconds</param>
        public void StartNewSectionFrom(double offset)
        {
            // The maximum strain of the new section is not zero by default, strain decays as usual regardless of section boundaries.
            // This means we need to capture the strain level at the beginning of the new section, and use that as the initial peak level.
            if (Previous.Count > 0)
                currentSectionPeak = currentStrain * strainDecay(offset - Previous[0].BaseObject.StartTime);
        }

        /// <summary>
        /// Returns the calculated difficulty value representing all processed <see cref="OsuDifficultyHitObject"/>s.
        /// </summary>
        public double DifficultyValue()
        {
            double difficulty = 0;

            // previously returned sum of 0.9^n * strain, which converges to 10*strain for constant strain.
            double legacyScalingFactor = 10;

            // difficulty to FC the remainder of the map from every position, used later to calculate expected map length. 
            var difficultyPartialSums = new List<double>();

            // Difficulty calculated according to probability of FC
            // iterate backwards to calculate difficultyPartialSums
            for (int i=strainPeaks.Count-1; i>=0; --i)
            {
                double strain = strainPeaks[i];
                double stars = Math.Sqrt(strain * legacyScalingFactor) * difficulty_multiplier;
                difficulty += Math.Exp(star_bonus_k * starsToDifficulty(stars)) * time_multiplier;
                difficultyPartialSums.Add(difficulty);
            }

            difficulty = Math.Log(difficulty) / star_bonus_k;

            double skill = getSkillToFcInTargetTime(difficulty, difficultyPartialSums);

            return difficultyToStars(skill);

        }

        /// <summary>
        /// Calculates the strain value of an <see cref="OsuDifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected abstract double StrainValueOf(OsuDifficultyHitObject current);

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        private double getSkillToFcInTargetTime(double difficulty, List<double> difficultyPartialSums)
        {
            // if map is really long, return skill level to pass with 0.5 probability
            if (difficultyPartialSums.Count >= target_fc_sections / 2)
                return skillLevel(0.5, difficulty);

            // otherwise return skill level to FC on average once per target_fc_time
            // hard to calculate exactly so approximate and iteratively improve. Normally gets to within 1% in 2 iterations

            // initial guess of average play length 
            double averageLength = difficultyPartialSums.Count * 0.3;
            double fcProb = averageLength / target_fc_sections;
            double skill = skillLevel(fcProb, difficulty);
            int max_iterations = 5;

            for (int i = 0; i < max_iterations; ++i)
            {
                // use estimate for improved average length calculation
                double expectedSectionsBeforeFC = getExpectedSectionsPlayedBeforeFC(skill, difficultyPartialSums);

                // play x sections per fc, fc with probability p per attempt, so on average play x*p sections per attempt
                averageLength = expectedSectionsBeforeFC * passProbability(skill, difficulty);

                fcProb = averageLength / target_fc_sections;
                skill = skillLevel(fcProb, difficulty);

                if (Math.Abs(expectedSectionsBeforeFC - target_fc_sections) / target_fc_sections < target_fc_precision)
                {
                    // enough precision already
                    break;
                }
            }

            // preserve star rating for averageLength = target_fc_base_time
            double target_fc_star_adjustment = -skillLevel(target_fc_base_time / target_fc_time, 0);

            return skill + target_fc_star_adjustment;
        }

        private double getExpectedSectionsPlayedBeforeFC(double skill, List<double> difficultyPartialSums)
        {
            // note: calculating this separately for each skill isn't really correct, maybe fix in future

            double length = 0;
            // difficultyPartialSums is a list of exp(k*d) for each strain where d is the difficulty to pass the remainder of the map
            foreach (double expDifficulty in difficultyPartialSums)
            {
                // if we fail before reaching this section, section playcount doesnt increase
                // it only increases if we fail afterwards
                double probabilityOfPassingRemainderOfMap = passProbFromExp(skill, expDifficulty);
                double expectedSectionPlaysBeforeFC = 1 / probabilityOfPassingRemainderOfMap;
                length += expectedSectionPlaysBeforeFC; 
            }

            return length;
        }

        // probability a player of the given skill passes a map of the given difficulty
        private double passProbability(double skill, double difficulty) => Math.Exp(-Math.Exp(-star_bonus_k * (skill - difficulty)));

        // same as above but pass in exp(k*difficulty) instead of difficulty
        private double passProbFromExp(double skill, double expDifficulty) => Math.Exp(-Math.Exp(-star_bonus_k * skill) * expDifficulty);

        // inverse of passProbability
        private double skillLevel(double probability, double difficulty) => difficulty - Math.Log(-Math.Log(probability)) / star_bonus_k;
 
        private double starsToDifficulty(double val)
        {
            if (val > 0)
                return (Math.Log(val));

            return -1e10;
        }

        private double difficultyToStars(double val)
        {
            return Math.Exp(val);
        }
    }
}
