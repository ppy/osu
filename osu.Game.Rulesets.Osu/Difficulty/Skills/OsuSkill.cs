// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using MathNet.Numerics.RootFinding;

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
        /// Repeating a section multiplies difficulty by this factor
        /// Increasing this number increases the impact of map length and decreases the impact of difficulty spikes on SR.
        /// </summary>
        protected virtual double DifficultyMultiplierPerRepeat => 1.0677;

        private double difficultyExponent => 1 / Math.Log(DifficultyMultiplierPerRepeat, 2);

        /// <summary>
        /// Constant difficulty sections of this length match old difficulty values.
        /// Decreasing this value increases star rating of all maps equally
        /// </summary>
        protected virtual double DifficultyBaseTime => (8.0 * 1000.0);

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
        private const double map_retry_time = 3 * 1000;

        /// <summary>
        /// Multiplier used to preserve star rating for maps with length <see cref="target_fc_base_time"/>
        /// </summary>
        private double targetFcDifficultyMultiplier => 1 / skillLevel(target_fc_base_time / target_retry_time_before_fc, 1);

        /// <summary>
        /// Size of lists used to interpolate combo difficulty value and miss count difficulty value for performance calculations.
        /// </summary>
        private const int difficulty_count = 20;

        private readonly List<NoteDifficultyData> noteDifficulties = new List<NoteDifficultyData>();

        private double totalPowDifficulty;
        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.

        public static readonly double[] MISS_STAR_RATING_MULTIPLIERS =
            Enumerable.Range(0, difficulty_count)
                      .Select(i => 1 - Math.Pow(i, 1.1) * 0.005)
                      .ToArray();

        public static readonly double[] COMBO_PERCENTAGES =
            Enumerable.Range(1, difficulty_count)
                      .Select(i => i / (double)difficulty_count)
                      .ToArray();

        public double[] MissCounts { get; private set; }
        public double[] ComboStarRatings { get; private set; }
        public double Difficulty { get; private set; }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values.
        /// Also calculates hit probability function for this note and adds to list
        /// </summary>
        public override void Process(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += StrainValueOf(current) * SkillMultiplier;

            addStrain(current, currentStrain);

            Previous.Push(current);
        }

        /// <summary>
        /// Perform difficulty calculations
        /// </summary>
        public override void Calculate()
        {
            Difficulty = Math.Pow(totalPowDifficulty, 1 / difficultyExponent);

            ComboStarRatings = calculateSkillToFcSubsets();
            MissCounts = calculateMissCounts(ComboStarRatings.Last());
        }

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        /// <summary>
        /// Get skill to fc easiest section with e.g. 5% combo, 10%, 15%, ... 100% combo.
        /// </summary>
        private double[] calculateSkillToFcSubsets()
        {
            return Enumerable.Range(1, difficulty_count).Select(i => targetFcDifficultyMultiplier * SkillToFcSectionCountInGivenTime(i)).ToArray();
        }

        /// <summary>
        /// Calculate miss count for a list of star ratings (used to evaluate miss count of plays).
        /// </summary>
        private double[] calculateMissCounts(double fcDifficulty)
        {
            var result = new double[difficulty_count];

            double fcSkill = fcDifficulty / targetFcDifficultyMultiplier;

            double fcProb = fcProbability(fcSkill, fcDifficulty);

            for (int i = 0; i < difficulty_count; ++i)
            {
                double missDifficulty = fcDifficulty * MISS_STAR_RATING_MULTIPLIERS[i];

                // skill is the same skill who can FC a missStars map with same length as this one in target_retry_time_until_fc
                double skill = missDifficulty / targetFcDifficultyMultiplier;

                double[] missProbs = getMissProbabilities(skill);

                result[i] = getMissCount(fcProb, missProbs);
            }

            return result;
        }

        /// <summary>
        /// Calculate the probability of missing each note given a skill level.
        /// </summary>
        private double[] getMissProbabilities(double skill)
        {
            var result = new double[noteDifficulties.Count];

            double powSkill = Math.Pow(Math.Max(skill, 1e-10), -difficultyExponent);

            for (int i = 0; i < noteDifficulties.Count; ++i)
            {
                result[i] = 1 - fcProbFromPow(powSkill, noteDifficulties[i].PowDifficulty);
            }

            return result;
        }

        /// <summary>
        /// Find first miss count achievable with at least probability p
        /// </summary>
        private double getMissCount(double p, double[] missProbabilities)
        {
            var distribution = new PoissonBinomial(missProbabilities);

            return Brent.FindRootExpand(missCount => distribution.Cdf(missCount) - p, 0, 20, 1e-4);
        }

        /// <summary>
        /// The probability a player of the given skill full combos a map of the given difficulty
        /// </summary>
        private double fcProbability(double skill, double difficulty) => Math.Exp(-Math.Pow(difficulty / Math.Max(1e-10, skill), difficultyExponent));

        /// <summary>
        /// Returns the same result as <see cref="fcProbability"/> with some values precomputed for efficiency
        /// </summary>
        /// <param name="powSkill">Skill of the player transformed with Math.Pow(skill, -k)</param>
        /// <param name="powDifficulty">Difficulty of a map/note/section transformed with Math.Pow(difficulty, k)</param>
        private double fcProbFromPow(double powSkill, double powDifficulty) => Math.Exp(-powSkill * powDifficulty);

        /// <summary>
        /// Player skill level that passes a map of the given difficulty with the given probability
        /// </summary>
        private double skillLevel(double probability, double difficulty) => difficulty * Math.Pow(-Math.Log(probability), -1 / difficultyExponent);

        private double difficultyForSubmap(NoteDifficultyData first, NoteDifficultyData last)
        {
            return Math.Pow(powDifficultyForSubmap(first, last), 1 / difficultyExponent);
        }

        private double powDifficultyForSubmap(NoteDifficultyData first, NoteDifficultyData last)
        {
            return last.CumulativePowDifficulty - first.CumulativePowDifficulty + first.PowDifficulty;
        }

        private void addStrain(DifficultyHitObject hitObject, double strain)
        {
            double strainDurationScale = Math.Min(MaxStrainTime, hitObject.DeltaTime) / DifficultyBaseTime;
            noteDifficulties.Add(new NoteDifficultyData(hitObject, strain, strainDurationScale, difficultyExponent, ref totalPowDifficulty));

            // add zero difficulty notes corresponding to slider ticks/slider ends so combo is reflected properly
            // (slider difficulty is currently handled in the following note)
            int extraNestedCount = hitObject.BaseObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; ++i)
            {
                noteDifficulties.Add(NoteDifficultyData.SliderTick(hitObject, totalPowDifficulty));
            }
        }

        public double SkillToFcSectionCountInGivenTime(int sectionCount)
        {
            double skill = estimateSkillForEasiestSubmap(sectionCount);

            for (int i = 0; i < 5; i++)
            {
                var performaceData = getPerformanceDataForEasiestSubmap(skill, sectionCount);

                if (Math.Abs(performaceData.ExpectedTimeUntilFullCombo - target_retry_time_before_fc) / target_retry_time_before_fc < target_fc_precision)
                {
                    // enough precision already
                    break;
                }

                double averageLength = performaceData.ExpectedTimeUntilFullCombo * performaceData.FullComboProbability;
                double newFcProb = averageLength / target_retry_time_before_fc;

                skill = skillLevel(newFcProb, Math.Pow(performaceData.PowDifficulty, 1 / difficultyExponent));
            }

            return skill;
        }

        private MapSectionPerformanceData getPerformanceDataForEasiestSubmap(double skill, int sectionCount)
        {
            var sectionData = getPerformanceDataForSections(skill);
            var easiestSectionPerformanceData = new MapSectionPerformanceData
            {
                ExpectedTimeUntilFullCombo = Double.PositiveInfinity,
            };

            for (int i = 0; i <= sectionData.Length - sectionCount; i++)
            {
                var sectionPerformanceData = aggregateMapSectionPerformanceData(sectionData, i, sectionCount);
                if (sectionPerformanceData.ExpectedTimeExcludingDuration < easiestSectionPerformanceData.ExpectedTimeExcludingDuration)
                    easiestSectionPerformanceData = sectionPerformanceData;
            }

            return easiestSectionPerformanceData;
        }

        private double estimateSkillForEasiestSubmap(int sectionCount)
        {
            double skill = Double.PositiveInfinity;

            for (int i = 0; i <= difficulty_count - sectionCount; i++)
            {
                int sectionStartIndex = noteDifficulties.Count * i / difficulty_count;
                int sectionEndIndex = noteDifficulties.Count * (i + sectionCount) / difficulty_count - 1;

                var first = noteDifficulties[sectionStartIndex];
                var last = noteDifficulties[sectionEndIndex];

                double averagePlayTimeEstimate = (last.Timestamp - first.PrevTimestamp) * 0.3;
                double difficulty = difficultyForSubmap(first, last);

                double fcProb = averagePlayTimeEstimate / target_retry_time_before_fc;

                skill = Math.Min(skillLevel(fcProb, difficulty), skill);
            }

            return skill;
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
        private MapSectionPerformanceData getPerformanceDataForSection(double skill, int first, int last)
        {
            double powSkill = Math.Pow(skill, -difficultyExponent);
            double powDifficulty = powDifficultyForSubmap(noteDifficulties[first], noteDifficulties[last]);

            var result = new MapSectionPerformanceData
            {
                StartTime = noteDifficulties[first].PrevTimestamp,
                EndTime = noteDifficulties[last].Timestamp,
                ExpectedTimeUntilFullCombo = 0,
                PowDifficulty = powDifficulty,
                FullComboProbability = fcProbFromPow(powSkill, powDifficulty)
            };

            for (int i = first; i <= last; ++i)
            {
                var note = noteDifficulties[i];
                double hitProbability = fcProbFromPow(powSkill, note.PowDifficulty) + 1e-10;
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
                result.PowDifficulty += sectionData[i].PowDifficulty;
            }

            return result;
        }

        /// <summary>
        /// Structure holding the expected performance parameters of a user with a given throughput (in terms of Fitts's law)
        /// on a given segment of a map.
        /// </summary>
        private struct MapSectionPerformanceData
        {
            /// <summary>
            /// The expected amount of time until a full combo is attained for the section.
            /// </summary>
            public double ExpectedTimeUntilFullCombo { get; set; }

            /// <summary>
            /// The expected probability of achieving a full combo on a single play-through of the section.
            /// </summary>
            public double FullComboProbability { get; set; }

            public double PowDifficulty { get; set; }

            /// <summary>
            /// Timestamp for the beginning of the section
            /// </summary>
            public double StartTime { get; set; }

            /// <summary>
            /// Timestamp for the end of the section
            /// </summary>
            public double EndTime { get; set; }

            /// <summary>
            /// Time taken to play the section one time
            /// </summary>
            public double Duration => EndTime - StartTime;

            /// <summary>
            /// Expected time taken to play all failed attempts
            /// </summary>
            public double ExpectedTimeExcludingDuration => ExpectedTimeUntilFullCombo - Duration;
        }
    }
}
