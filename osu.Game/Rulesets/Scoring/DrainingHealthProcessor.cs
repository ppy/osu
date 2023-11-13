// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A <see cref="HealthProcessor"/> which continuously drains health.<br />
    /// At HP=0, the minimum health reached for a perfect play is 95%.<br />
    /// At HP=5, the minimum health reached for a perfect play is 70%.<br />
    /// At HP=10, the minimum health reached for a perfect play is 30%.
    /// </summary>
    public partial class DrainingHealthProcessor : HealthProcessor
    {
        /// <summary>
        /// A reasonable allowable error for the minimum health offset from <see cref="targetMinimumHealth"/>. A 1% error is unnoticeable.
        /// </summary>
        private const double minimum_health_error = 0.01;

        /// <summary>
        /// The minimum health target at an HP drain rate of 0.
        /// </summary>
        private const double min_health_target = 0.99;

        /// <summary>
        /// The minimum health target at an HP drain rate of 5.
        /// </summary>
        private const double mid_health_target = 0.9;

        /// <summary>
        /// The minimum health target at an HP drain rate of 10.
        /// </summary>
        private const double max_health_target = 0.4;

        /// <summary>
        /// The drain rate as a proportion of the total health drained per millisecond.
        /// </summary>
        public double DrainRate { get; private set; } = 1;

        /// <summary>
        /// The beatmap.
        /// </summary>
        protected IBeatmap Beatmap { get; private set; }

        /// <summary>
        /// The time at which health starts draining.
        /// </summary>
        protected readonly double DrainStartTime;

        /// <summary>
        /// An amount of lenience to apply to the drain rate.
        /// </summary>
        protected readonly double DrainLenience;

        private readonly List<(double time, double health)> healthIncreases = new List<(double, double)>();
        private double gameplayEndTime;
        private double targetMinimumHealth;

        private PeriodTracker noDrainPeriodTracker;

        /// <summary>
        /// Creates a new <see cref="DrainingHealthProcessor"/>.
        /// </summary>
        /// <param name="drainStartTime">The time after which draining should begin.</param>
        /// <param name="drainLenience">A lenience to apply to the default drain rate.<br />
        /// A value of 0 uses the default drain rate.<br />
        /// A value of 0.5 halves the drain rate.<br />
        /// A value of 1 completely removes drain.</param>
        public DrainingHealthProcessor(double drainStartTime, double drainLenience = 0)
        {
            DrainStartTime = drainStartTime;
            DrainLenience = Math.Clamp(drainLenience, 0, 1);
        }

        protected override void Update()
        {
            base.Update();

            if (noDrainPeriodTracker?.IsInAny(Time.Current) == true)
                return;

            // When jumping in and out of gameplay time within a single frame, health should only be drained for the period within the gameplay time
            double lastGameplayTime = Math.Clamp(Time.Current - Time.Elapsed, DrainStartTime, gameplayEndTime);
            double currentGameplayTime = Math.Clamp(Time.Current, DrainStartTime, gameplayEndTime);

            if (DrainLenience < 1)
                Health.Value -= DrainRate * (currentGameplayTime - lastGameplayTime);
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            Beatmap = beatmap;

            if (beatmap.HitObjects.Count > 0)
                gameplayEndTime = beatmap.HitObjects[^1].GetEndTime();

            noDrainPeriodTracker = new PeriodTracker(
                beatmap.Breaks.Select(breakPeriod =>
                    new Period(
                        beatmap.HitObjects
                               .Select(hitObject => hitObject.GetEndTime())
                               .Where(endTime => endTime <= breakPeriod.StartTime)
                               .DefaultIfEmpty(double.MinValue)
                               .Last(),
                        beatmap.HitObjects
                               .Select(hitObject => hitObject.StartTime)
                               .Where(startTime => startTime >= breakPeriod.EndTime)
                               .DefaultIfEmpty(double.MaxValue)
                               .First()
                    )));

            targetMinimumHealth = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, min_health_target, mid_health_target, max_health_target);

            // Add back a portion of the amount of HP to be drained, depending on the lenience requested.
            targetMinimumHealth += DrainLenience * (1 - targetMinimumHealth);

            // Ensure the target HP is within an acceptable range.
            targetMinimumHealth = Math.Clamp(targetMinimumHealth, 0, 1);

            base.ApplyBeatmap(beatmap);
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            base.ApplyResultInternal(result);

            if (!result.Type.IsBonus())
                healthIncreases.Add((result.HitObject.GetEndTime() + result.TimeOffset, GetHealthIncreaseFor(result)));
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            DrainRate = 1;

            if (storeResults)
                DrainRate = ComputeDrainRate();

            healthIncreases.Clear();
        }

        protected virtual double ComputeDrainRate()
        {
            if (healthIncreases.Count <= 1)
                return 0;

            int adjustment = 1;
            double result = 1;

            // Although we expect the following loop to converge within 30 iterations (health within 1/2^31 accuracy of the target),
            // we'll still keep a safety measure to avoid infinite loops by detecting overflows.
            while (adjustment > 0)
            {
                double currentHealth = 1;
                double lowestHealth = 1;
                int currentBreak = 0;

                for (int i = 0; i < healthIncreases.Count; i++)
                {
                    double currentTime = healthIncreases[i].time;
                    double lastTime = i > 0 ? healthIncreases[i - 1].time : DrainStartTime;

                    while (currentBreak < Beatmap.Breaks.Count && Beatmap.Breaks[currentBreak].EndTime <= currentTime)
                    {
                        // If two hitobjects are separated by a break period, there is no drain for the full duration between the hitobjects.
                        // This differs from legacy (version < 8) beatmaps which continue draining until the break section is entered,
                        // but this shouldn't have a noticeable impact in practice.
                        lastTime = currentTime;
                        currentBreak++;
                    }

                    // Apply health adjustments
                    currentHealth -= (currentTime - lastTime) * result;
                    lowestHealth = Math.Min(lowestHealth, currentHealth);
                    currentHealth = Math.Min(1, currentHealth + healthIncreases[i].health);

                    // Common scenario for when the drain rate is definitely too harsh
                    if (lowestHealth < 0)
                        break;
                }

                // Stop if the resulting health is within a reasonable offset from the target
                if (Math.Abs(lowestHealth - targetMinimumHealth) <= minimum_health_error)
                    break;

                // This effectively works like a binary search - each iteration the search space moves closer to the target, but may exceed it.
                adjustment *= 2;
                result += 1.0 / adjustment * Math.Sign(lowestHealth - targetMinimumHealth);
            }

            return result;
        }
    }
}
