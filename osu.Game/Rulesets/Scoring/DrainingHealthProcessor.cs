// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A <see cref="HealthProcessor"/> which continuously drains health.
    /// </summary>
    public class DrainingHealthProcessor : HealthProcessor
    {
        private IBeatmap beatmap;

        private double gameplayEndTime;

        private List<(double time, double health)> healthIncreases;
        private double targetMinimumHealth;
        private double drainRate = 1;

        /// <summary>
        /// Creates a new <see cref="DrainingHealthProcessor"/>.
        /// </summary>
        /// <param name="gameplayStartTime">The gameplay start time.</param>
        public DrainingHealthProcessor(double gameplayStartTime)
            : base(gameplayStartTime)
        {
        }

        protected override void Update()
        {
            base.Update();

            if (!IsBreakTime.Value)
            {
                // When jumping in and out of gameplay time within a single frame, health should only be drained for the period within the gameplay time
                double lastGameplayTime = Math.Clamp(Time.Current - Time.Elapsed, GameplayStartTime, gameplayEndTime);
                double currentGameplayTime = Math.Clamp(Time.Current, GameplayStartTime, gameplayEndTime);

                Health.Value -= drainRate * (currentGameplayTime - lastGameplayTime);
            }
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            this.beatmap = beatmap;

            if (beatmap.HitObjects.Count > 0)
                gameplayEndTime = beatmap.HitObjects[^1].GetEndTime();

            healthIncreases = new List<(double time, double health)>();
            targetMinimumHealth = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.95, 0.70, 0.30);

            base.ApplyBeatmap(beatmap);

            // Only required during the simulation stage
            healthIncreases = null;
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            base.ApplyResultInternal(result);
            healthIncreases?.Add((result.HitObject.GetEndTime() + result.TimeOffset, GetHealthIncreaseFor(result)));
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            drainRate = 1;

            if (storeResults)
            {
                int count = 1;

                while (true)
                {
                    double currentHealth = 1;
                    double lowestHealth = 1;
                    int currentBreak = -1;

                    for (int i = 0; i < healthIncreases.Count; i++)
                    {
                        double currentTime = healthIncreases[i].time;
                        double lastTime = i > 0 ? healthIncreases[i - 1].time : GameplayStartTime;

                        // Subtract any break time from the duration since the last object
                        if (beatmap.Breaks.Count > 0)
                        {
                            while (currentBreak + 1 < beatmap.Breaks.Count && beatmap.Breaks[currentBreak + 1].EndTime < currentTime)
                                currentBreak++;

                            if (currentBreak >= 0)
                                lastTime = Math.Max(lastTime, beatmap.Breaks[currentBreak].EndTime);
                        }

                        // Apply health adjustments
                        currentHealth -= (healthIncreases[i].time - lastTime) * drainRate;
                        lowestHealth = Math.Min(lowestHealth, currentHealth);
                        currentHealth = Math.Min(1, currentHealth + healthIncreases[i].health);

                        // Common scenario for when the drain rate is definitely too harsh
                        if (lowestHealth < 0)
                            break;
                    }

                    if (Math.Abs(lowestHealth - targetMinimumHealth) <= 0.01)
                        break;

                    count *= 2;
                    drainRate += 1.0 / count * Math.Sign(lowestHealth - targetMinimumHealth);
                }
            }

            healthIncreases.Clear();
        }
    }
}
