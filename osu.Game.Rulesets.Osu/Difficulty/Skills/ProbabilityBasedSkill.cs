// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using MathNet.Numerics.RootFinding;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Used to process strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class ProbabilityBasedSkill : StrainSkill
    {
        /// <summary>
        /// The maximum allowable strain time for a single note.
        /// </summary>
        protected virtual double MaxStrainTime => 200;

        /// <summary>
        /// Repeating a section one time multiplies difficulty by this factor.
        /// Increasing this number increases the impact of map length and decreases the impact of difficulty spikes on star rating.
        /// </summary>
        protected virtual double DifficultyMultiplierPerRepeat => 1.0677;

        private double difficultyExponent => 1 / Math.Log(DifficultyMultiplierPerRepeat, 2);

        /// <summary>
        /// Constant difficulty sections of this length match old difficulty values.
        /// Decreasing this value increases star rating of all maps equally.
        /// </summary>
        protected virtual double DifficultyBaseTime => 8.0 * 1000.0;

        /// <summary>
        /// The final star rating is supposed to be based on the skill level of a player who can FC the map once
        /// in this amount of time (in milliseconds).
        /// Decrease this number to give longer maps more PP.
        /// </summary>
        private const double target_retry_time_before_fc = 3.5 * 60 * 60 * 1000;

        /// <summary>
        /// Minimum allowable precision for the computation of the expected time spent before a player completes a full combo of the map.
        /// </summary>
        /// <remarks>
        /// Typically will be around 5x more precise.
        /// Current setting of 0.05 usually takes 2 iterations and gives around 4 digits of precision for star ratings.
        /// </remarks>
        private const double target_fc_precision = 0.05;

        /// <summary>
        /// Maximum number of iterations allowed when estimating the full combo probability of a section.
        /// </summary>
        private const int max_fc_estimation_iterations = 5;

        /// <summary>
        /// Maps with this expected length will match legacy PP values.
        /// Decrease this value to increase PP for all maps equally.
        /// </summary>
        private const double target_fc_base_time = 33 * 1000;

        /// <summary>
        /// Approximated amount of time needed to retry and get to the beginning of a map (in milliseconds).
        /// Increasing adds more weight to the first few notes of a map when calculating expected time to FC.
        /// </summary>
        private const double map_retry_time = 3 * 1000;

        /// <summary>
        /// Multiplier used to preserve star rating for maps with length of <see cref="target_fc_base_time"/>.
        /// </summary>
        private double fullComboDifficultyMultiplier => 1 / skillLevel(target_fc_base_time / target_retry_time_before_fc, 1);

        /// <summary>
        /// Size of lists used to interpolate combo difficulty value and miss count difficulty value for performance calculations.
        /// </summary>
        private const int difficulty_count = 20;

        private readonly List<NoteDifficultyData> noteDifficulties = new List<NoteDifficultyData>();

        /// <summary>
        /// The current total exponential difficulty of the map.
        /// </summary>
        private double currentTotalExponentialDifficulty;

        /// <summary>
        /// The current strain level.
        /// </summary>
        private double currentStrain = 1;

        public static readonly double[] COMBO_PERCENTAGES =
            Enumerable.Range(1, difficulty_count)
                      .Select(i => i / (double)difficulty_count)
                      .ToArray();

        /// <summary>
        /// Contains a list of star rating values that indicate how hard it is to full combo the easiest X% of the map,
        /// where values of X are taken from <see cref="COMBO_PERCENTAGES"/>.
        /// </summary>
        public double[] ComboStarRatings { get; private set; }

        public static readonly double[] MISS_STAR_RATING_MULTIPLIERS =
            Enumerable.Range(0, difficulty_count)
                      .Select(i => 1 - Math.Pow(i, 1.1) * 0.005)
                      .ToArray();

        /// <summary>
        /// Contains the expected numbers of misses from players whose skill level allows them to FC maps
        /// that are <see cref="MISS_STAR_RATING_MULTIPLIERS"/> times as hard as the FC of the currently considered map.
        /// </summary>
        public double[] MissCounts { get; private set; }

        protected ProbabilityBasedSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values.
        /// </summary>
        protected override void Process(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += StrainValueOf(current) * SkillMultiplier;

            addStrain(current, currentStrain);
        }

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        private void addStrain(DifficultyHitObject hitObject, double strain)
        {
            double strainDurationScale = Math.Min(MaxStrainTime, hitObject.DeltaTime) / DifficultyBaseTime;
            noteDifficulties.Add(new NoteDifficultyData(hitObject, strain, strainDurationScale, difficultyExponent, ref currentTotalExponentialDifficulty));

            // add zero difficulty notes corresponding to slider ticks/slider ends so combo is reflected properly
            // (slider difficulty is currently handled in the following note)
            int extraNestedCount = hitObject.BaseObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; ++i)
            {
                noteDifficulties.Add(NoteDifficultyData.SliderTick(hitObject, currentTotalExponentialDifficulty));
            }
        }

        /// <summary>
        /// Complete calculations of the difficulty with regard to this skill.
        /// </summary>
        public override double DifficultyValue()
        {
            ComboStarRatings = calculateMinimumStarRatingsForSections();
            MissCounts = calculateMissCounts(ComboStarRatings.Last());

            return Math.Pow(currentTotalExponentialDifficulty, 1 / difficultyExponent);
        }

        /// <summary>
        /// Returns the expected star ratings of the easiest N sections of the map, where N is in range [1, <see cref="difficulty_count"/>].
        /// The returned value corresponds directly to <see cref="ComboStarRatings"/>.
        /// </summary>
        private double[] calculateMinimumStarRatingsForSections()
        {
            return Enumerable.Range(1, difficulty_count)
                             .Select(i => fullComboDifficultyMultiplier * requiredSkillToFullComboEasiestSubmap(i)).ToArray();
        }

        /// <summary>
        /// Estimates the skill required to FC the easiest submap.
        /// A submap is defined as the easiest <paramref name="sectionCount"/> consecutive sections of the map.
        /// </summary>
        private double requiredSkillToFullComboEasiestSubmap(int sectionCount)
        {
            double skill = estimateSkillForEasiestSubmap(sectionCount);

            for (int i = 0; i < max_fc_estimation_iterations; i++)
            {
                var performanceData = getPerformanceDataForEasiestSubmap(skill, sectionCount);

                if (Math.Abs(performanceData.ExpectedTimeUntilFullCombo - target_retry_time_before_fc) / target_retry_time_before_fc < target_fc_precision)
                {
                    // enough precision already
                    break;
                }

                double averageLength = performanceData.ExpectedTimeUntilFullCombo * performanceData.FullComboProbability;
                double newFcProb = averageLength / target_retry_time_before_fc;

                skill = skillLevel(newFcProb, Math.Pow(performanceData.ExponentiatedDifficulty, 1 / difficultyExponent));
            }

            return skill;
        }

        private double estimateSkillForEasiestSubmap(int sectionCount)
        {
            double skill = double.PositiveInfinity;

            for (int i = 0; i <= difficulty_count - sectionCount; i++)
            {
                int sectionStartIndex = noteDifficulties.Count * i / difficulty_count;
                int sectionEndIndex = noteDifficulties.Count * (i + sectionCount) / difficulty_count - 1;

                var first = noteDifficulties[sectionStartIndex];
                var last = noteDifficulties[sectionEndIndex];

                double averagePlayTimeEstimate = (last.StartTime - first.PreviousStartTime) * 0.3;
                double difficulty = mapSectionDifficulty(first, last);

                double fcProb = averagePlayTimeEstimate / target_retry_time_before_fc;

                skill = Math.Min(skillLevel(fcProb, difficulty), skill);
            }

            return skill;
        }

        /// <summary>
        /// Returns performance data for a player with the given <paramref name="skill"/>
        /// playing the easiest submap of length <paramref name="sectionCount"/>.
        /// </summary>
        private MapSectionPerformanceData getPerformanceDataForEasiestSubmap(double skill, int sectionCount)
        {
            var sectionData = getPerformanceDataForSections(skill);
            var easiestSectionPerformanceData = new MapSectionPerformanceData
            {
                ExpectedTimeUntilFullCombo = double.PositiveInfinity,
            };

            for (int i = 0; i <= sectionData.Length - sectionCount; i++)
            {
                var sectionPerformanceData = aggregateMapSectionPerformanceData(sectionData, i, sectionCount);
                if (sectionPerformanceData.ExpectedDurationOfFailedFullComboAttempts < easiestSectionPerformanceData.ExpectedDurationOfFailedFullComboAttempts)
                    easiestSectionPerformanceData = sectionPerformanceData;
            }

            return easiestSectionPerformanceData;
        }

        private MapSectionPerformanceData[] getPerformanceDataForSections(double skill)
        {
            var result = new MapSectionPerformanceData[difficulty_count];

            for (int i = 0; i < difficulty_count; i++)
            {
                int sectionStartIndex = noteDifficulties.Count * i / difficulty_count;
                int sectionEndIndex = noteDifficulties.Count * (i + 1) / difficulty_count - 1;

                result[i] = getPerformanceDataForSection(skill, sectionStartIndex, sectionEndIndex);
            }

            return result;
        }

        /// <summary>
        /// Calculate the average time a player with the given skill will take to Full Combo the given section of a map.
        /// </summary>
        /// <param name="skill">The skill level of the player.</param>
        /// <param name="first">Index of the first object in the section.</param>
        /// <param name="last">Index of the last object in the section.</param>
        private MapSectionPerformanceData getPerformanceDataForSection(double skill, int first, int last)
        {
            double powSkill = Math.Pow(skill, -difficultyExponent);
            double powDifficulty = exponentiatedMapSectionDifficulty(noteDifficulties[first], noteDifficulties[last]);

            var result = new MapSectionPerformanceData
            {
                StartTime = noteDifficulties[first].PreviousStartTime,
                EndTime = noteDifficulties[last].StartTime,
                ExpectedTimeUntilFullCombo = 0,
                ExponentiatedDifficulty = powDifficulty,
                FullComboProbability = fcProbabilityPrecomputed(powSkill, powDifficulty)
            };

            for (int i = first; i <= last; ++i)
            {
                var note = noteDifficulties[i];
                double hitProbability = fcProbabilityPrecomputed(powSkill, note.ExponentiatedDifficulty) + 1e-10;
                result.ExpectedTimeUntilFullCombo = (result.ExpectedTimeUntilFullCombo + note.DeltaTime) / hitProbability;
            }

            return result;
        }

        /// <summary>
        /// The estimated time it takes a player to full-combo a particular submap,
        /// assuming they retry as soon as they miss a note.
        /// </summary>
        private MapSectionPerformanceData aggregateMapSectionPerformanceData(MapSectionPerformanceData[] sectionData, int first, int count)
        {
            int last = first + count - 1;

            // TODO: clarify - is this just the approximate time needed to restart the map?
            var result = new MapSectionPerformanceData
            {
                ExpectedTimeUntilFullCombo = map_retry_time,
                FullComboProbability = 1,
                StartTime = sectionData[first].StartTime,
                EndTime = sectionData[last].EndTime
            };

            for (int i = first; i <= last; ++i)
            {
                // simplification - assumes that two sections being hit are independent events
                // (not necessarily the case, but that is hard to estimate objectively)
                result.ExpectedTimeUntilFullCombo /= sectionData[i].FullComboProbability;
                result.ExpectedTimeUntilFullCombo += sectionData[i].ExpectedTimeUntilFullCombo;
                result.FullComboProbability *= sectionData[i].FullComboProbability;
                result.ExponentiatedDifficulty += sectionData[i].ExponentiatedDifficulty;
            }

            return result;
        }

        /// <summary>
        /// Estimates the number of misses expected from players that can FC maps <see cref="MISS_STAR_RATING_MULTIPLIERS"/> as hard as the current one.
        /// The returned value corresponds directly to <see cref="MissCounts"/>.
        /// </summary>
        /// <param name="fullComboDifficulty">The star difficulty of a full combo on the current map.</param>
        private double[] calculateMissCounts(double fullComboDifficulty)
        {
            var result = new double[difficulty_count];

            double fcSkill = fullComboDifficulty / fullComboDifficultyMultiplier;
            double fcProb = fcProbability(fcSkill, fullComboDifficulty);

            for (int i = 0; i < difficulty_count; ++i)
            {
                double missDifficulty = fullComboDifficulty * MISS_STAR_RATING_MULTIPLIERS[i];
                double missSkill = missDifficulty / fullComboDifficultyMultiplier;

                double[] missProbabilities = getMissProbabilities(missSkill);
                result[i] = getMissCount(fcProb, missProbabilities);
            }

            return result;
        }

        /// <summary>
        /// Calculate the probability of missing each note given a skill level.
        /// </summary>
        /// <param name="skill">The skill level of the playing user.</param>
        private double[] getMissProbabilities(double skill)
        {
            var result = new double[noteDifficulties.Count];

            double powSkill = Math.Pow(Math.Max(skill, 1e-10), -difficultyExponent);

            for (int i = 0; i < noteDifficulties.Count; ++i)
            {
                result[i] = 1 - fcProbabilityPrecomputed(powSkill, noteDifficulties[i].ExponentiatedDifficulty);
            }

            return result;
        }

        /// <summary>
        /// Find first miss count achievable with at least probability p
        /// </summary>
        private double getMissCount(double p, double[] missProbabilities)
        {
            var distribution = new PoissonBinomial(missProbabilities);

            return Brent.FindRootExpand(missCount => distribution.CDF(missCount) - p, 0, 20, 1e-4);
        }

        /// <summary>
        /// Calculates the difficulty of a map section starting with <paramref name="first"/>, and ending with <paramref name="last"/>.
        /// </summary>
        /// <param name="first">The first note of the section.</param>
        /// <param name="last">The last note of the section.</param>
        private double mapSectionDifficulty(NoteDifficultyData first, NoteDifficultyData last)
        {
            return Math.Pow(exponentiatedMapSectionDifficulty(first, last), 1 / difficultyExponent);
        }

        /// <summary>
        /// Calculates the exponential difficulty of a map section starting with <paramref name="first"/>, and ending with <paramref name="last"/>.
        /// </summary>
        /// <param name="first">The first note of the section.</param>
        /// <param name="last">The last note of the section.</param>
        private double exponentiatedMapSectionDifficulty(NoteDifficultyData first, NoteDifficultyData last)
        {
            return last.TotalExponentiatedDifficulty - first.TotalExponentiatedDifficulty + first.ExponentiatedDifficulty;
        }

        /// <summary>
        /// The probability a player of the given skill full combos a map of the given difficulty.
        /// </summary>
        /// <param name="skill">The skill level of the player.</param>
        /// <param name="difficulty">The difficulty of a range of notes.</param>
        private double fcProbability(double skill, double difficulty) => Math.Exp(-Math.Pow(difficulty / Math.Max(1e-10, skill), difficultyExponent));

        /// <summary>
        /// Returns the same result as <see cref="fcProbability"/> with some values precomputed for efficiency.
        /// </summary>
        /// <param name="powSkill">The skill level of the player, raised to negative-<see cref="difficultyExponent"/>-th power.</param>
        /// <param name="powDifficulty">The difficulty of a range of notes, raised to the <see cref="difficultyExponent"/>-th power.</param>
        private double fcProbabilityPrecomputed(double powSkill, double powDifficulty) => Math.Exp(-powDifficulty * powSkill);

        /// <summary>
        /// Approximates the skill level of a player that can FC a map with the given <paramref name="difficulty"/>,
        /// if their probability of success in doing so is equal to <paramref name="probability"/>.
        /// </summary>
        private double skillLevel(double probability, double difficulty) => difficulty * Math.Pow(-Math.Log(probability), -1 / difficultyExponent);
    }
}
