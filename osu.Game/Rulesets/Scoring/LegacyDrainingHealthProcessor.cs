// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Game.Beatmaps;
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
    public abstract partial class LegacyDrainingHealthProcessor : HealthProcessor
    {
        protected double DrainStartTime { get; }
        protected double GameplayEndTime { get; private set; }

        protected IBeatmap Beatmap { get; private set; }
        protected PeriodTracker NoDrainPeriodTracker { get; private set; }

        public bool Log { get; set; }

        public double DrainRate { get; private set; }

        /// <summary>
        /// Creates a new <see cref="DrainingHealthProcessor"/>.
        /// </summary>
        /// <param name="drainStartTime">The time after which draining should begin.</param>
        protected LegacyDrainingHealthProcessor(double drainStartTime)
        {
            DrainStartTime = drainStartTime;
        }

        protected override void Update()
        {
            base.Update();

            if (NoDrainPeriodTracker?.IsInAny(Time.Current) == true)
                return;

            // When jumping in and out of gameplay time within a single frame, health should only be drained for the period within the gameplay time
            double lastGameplayTime = Math.Clamp(Time.Current - Time.Elapsed, DrainStartTime, GameplayEndTime);
            double currentGameplayTime = Math.Clamp(Time.Current, DrainStartTime, GameplayEndTime);

            Health.Value -= DrainRate * (currentGameplayTime - lastGameplayTime);
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            Beatmap = beatmap;

            if (beatmap.HitObjects.Count > 0)
                GameplayEndTime = beatmap.HitObjects[^1].GetEndTime();

            NoDrainPeriodTracker = new PeriodTracker(beatmap.Breaks.Select(breakPeriod => new Period(
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

            base.ApplyBeatmap(beatmap);
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            DrainRate = 1;

            if (storeResults)
                DrainRate = ComputeDrainRate();
        }

        protected abstract double ComputeDrainRate();
    }
}
