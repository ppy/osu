// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// Helper class used to estimate probabilities of hitting movements in a map.
    /// </summary>
    internal class HitProbabilities
    {
        /// <summary>
        /// Probability data collected for each subsection of the map.
        /// </summary>
        private readonly MapSectionProbabilities[] sections;

        /// <summary>
        /// TODO: explain
        /// </summary>
        /// <param name="movements">The list of all <see cref="OsuMovement"/>s in the map being considered.</param>
        /// <param name="cheeseLevel">TODO: explain</param>
        /// <param name="sectionCount">The number of sections to subdivide the map into.</param>
        public HitProbabilities(List<OsuMovement> movements, double cheeseLevel, int sectionCount = 20)
        {
            sections = new MapSectionProbabilities[sectionCount];

            for (int i = 0; i < sectionCount; ++i)
            {
                int sectionStartIndex = movements.Count * i / sectionCount;
                int sectionEndIndex = movements.Count * (i + 1) / sectionCount - 1;

                double sectionStartTime = movements[sectionStartIndex].StartTime;
                double sectionEndTime = movements[sectionEndIndex].StartTime;

                var sectionMovements = movements.GetRange(sectionStartIndex, sectionEndIndex - sectionStartIndex + 1);
                sections[i] = new MapSectionProbabilities(sectionMovements, cheeseLevel, sectionStartTime, sectionEndTime);
            }
        }

        /// <summary>
        /// Estimates the probability of hitting the given <paramref name="movement"/>, with added penalty for cheesable
        /// movements given by <paramref name="cheeseLevel"/>.
        /// </summary>
        /// <param name="movement">The movement being performed by the player.</param>
        /// <param name="throughput">
        /// The assumed throughput of the player in terms of Fitts's law
        /// (functioning as a skill measure).
        /// </param>
        /// <param name="cheeseLevel">The penalty to apply to cheesable movements.</param>
        public static double GetHitProbabilityAdjustedForCheese(OsuMovement movement, double throughput, double cheeseLevel)
        {
            double perMovementCheeseLevel = cheeseLevel;

            if (movement.EndsOnSlider)
                perMovementCheeseLevel = 0.5 * cheeseLevel + 0.5;

            // if the movement can be cheesed, movement time is increased, resulting in a difficulty decrease
            double adjustedMovementTime = movement.MovementTime * (1 + perMovementCheeseLevel * movement.CheeseWindow);
            return FittsLaw.ProbabilityToHit(movement.Distance, adjustedMovementTime, throughput);
        }

        /// <summary>
        /// Approximates the minimum amount of time required to get a full combo on a submap of length <paramref name="sectionCount"/>.
        /// </summary>
        /// <remarks>
        /// A submap of length <paramref name="sectionCount"/> is a portion of the map, which consists of <paramref name="sectionCount"/>
        /// consecutive <see cref="sections"/>.
        /// </remarks>
        /// <param name="throughput">
        /// The assumed throughput of the player in terms of Fitts's law
        /// (functioning as a skill measure).
        /// </param>
        /// <param name="sectionCount">The number of consecutive sections to consider for the submaps.</param>
        public double MinimumTimeForFullComboOnSubmap(double throughput, int sectionCount)
        {
            double fcTime = double.PositiveInfinity;

            var sectionData = sections.Select(x => x.Evaluate(throughput)).ToArray();

            for (int i = 0; i <= sections.Length - sectionCount; i++)
            {
                fcTime = Math.Min(fcTime, expectedTimeToFullCombo(sectionData, i, sectionCount) - submapLength(i, sectionCount));
            }

            return fcTime;
        }

        /// <summary>
        /// Calculates the duration of a submap starting at the section at index <paramref name="start"/>
        /// and spanning <paramref name="sectionCount"/> sections.
        /// </summary>
        private double submapLength(int start, int sectionCount)
            => sections[start + sectionCount - 1].EndTime - sections[start].StartTime;

        /// <summary>
        /// The estimated time it takes a player to full-combo a particular submap,
        /// assuming they retry as soon as they miss a note.
        /// </summary>
        private static double expectedTimeToFullCombo(MapSectionPerformanceData[] sectionData, int start, int count)
        {
            // TODO: clarify - is this just the approximate time needed to restart the map?
            double fcTime = 15;

            for (int i = start; i < start + count; ++i)
            {
                // simplification - assumes that two sections being hit are independent events
                // (not necessarily the case, but that is hard to estimate objectively)
                fcTime /= sectionData[i].FullComboProbability;
                fcTime += sectionData[i].ExpectedTimeUntilFullCombo;
            }

            return fcTime;
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
            public double ExpectedTimeUntilFullCombo;

            /// <summary>
            /// The expected probability of achieving a full combo on a single play-through of the section.
            /// </summary>
            public double FullComboProbability;
        }

        /// <summary>
        /// Holds the probability data concerning a single section of the map.
        /// </summary>
        private class MapSectionProbabilities
        {
            /// <summary>
            /// Section performance metrics computed previously.
            /// The key is the assumed user throughput, the value contains the performance parameters for this section
            /// given that throughput.
            /// </summary>
            private readonly Dictionary<double, MapSectionPerformanceData> cache = new Dictionary<double, MapSectionPerformanceData>();

            /// <summary>
            /// TODO: explain
            /// </summary>
            private readonly double cheeseLevel;

            /// <summary>
            /// The start time of the map section.
            /// </summary>
            public readonly double StartTime;

            /// <summary>
            /// The end time of the map section.
            /// </summary>
            public readonly double EndTime;

            /// <summary>
            /// The movements to be performed by the user in this section.
            /// </summary>
            public List<OsuMovement> Movements { get; }

            public MapSectionProbabilities(List<OsuMovement> movements, double cheeseLevel, double startTime, double endTime)
            {
                Movements = movements;
                StartTime = startTime;
                EndTime = endTime;

                this.cheeseLevel = cheeseLevel;
            }

            /// <summary>
            /// Estimates user performance on this section of the map.
            /// </summary>
            /// <param name="throughput">The assumed throughput of the user (serving as a skill measure).</param>
            public MapSectionPerformanceData Evaluate(double throughput)
            {
                if (Movements.Count == 0)
                    return new MapSectionPerformanceData { ExpectedTimeUntilFullCombo = 0, FullComboProbability = 1 };

                if (cache.TryGetValue(throughput, out MapSectionPerformanceData result))
                    return result;

                result.ExpectedTimeUntilFullCombo = 0;
                result.FullComboProbability = 1;

                foreach (OsuMovement movement in Movements)
                {
                    double hitProbability = GetHitProbabilityAdjustedForCheese(movement, throughput, cheeseLevel) + 1e-10;

                    // very low hit probabilities are rescaled/increased somewhat, as a nerf.
                    hitProbability = 1 - (Math.Sqrt(1 - hitProbability + 0.25) - 0.5);

                    // simplification - consecutive movements are considered to be probabilistically independent events
                    result.ExpectedTimeUntilFullCombo = (result.ExpectedTimeUntilFullCombo + movement.RawMovementTime) / hitProbability;
                    result.FullComboProbability *= hitProbability;
                }

                cache.Add(throughput, result);

                return result;
            }
        }
    }
}
