// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    public class HealthProcessor : JudgementProcessor
    {
        /// <summary>
        /// Invoked when the <see cref="ScoreProcessor"/> is in a failed state.
        /// Return true if the fail was permitted.
        /// </summary>
        public event Func<bool> Failed;

        /// <summary>
        /// Additional conditions on top of <see cref="DefaultFailCondition"/> that cause a failing state.
        /// </summary>
        public event Func<HealthProcessor, JudgementResult, bool> FailConditions;

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Whether gameplay is currently in a break.
        /// </summary>
        public readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed state.
        /// </summary>
        public bool HasFailed { get; private set; }

        private readonly double gameplayStartTime;

        private IBeatmap beatmap;

        private List<(double time, double health)> healthIncreases;
        private double targetMinimumHealth;
        private double drainRate = 1;

        public HealthProcessor(double gameplayStartTime)
        {
            this.gameplayStartTime = gameplayStartTime;
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            this.beatmap = beatmap;

            healthIncreases = new List<(double time, double health)>();
            targetMinimumHealth = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.95, 0.6, 0.2);

            base.ApplyBeatmap(beatmap);

            // Only required during the simulation stage
            healthIncreases = null;
        }

        protected override void Update()
        {
            base.Update();

            if (!IsBreakTime.Value)
                Health.Value -= drainRate * Time.Elapsed;
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            double healthIncrease = result.Judgement.HealthIncreaseFor(result);
            healthIncreases?.Add((result.HitObject.GetEndTime() + result.TimeOffset, healthIncrease));

            if (HasFailed)
                return;

            Health.Value += healthIncrease;

            if (!DefaultFailCondition && FailConditions?.Invoke(this, result) != true)
                return;

            if (Failed?.Invoke() != false)
                HasFailed = true;
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            Health.Value = result.HealthAtJudgement;

            // Todo: Revert HasFailed state with proper player support
        }

        /// <summary>
        /// The default conditions for failing.
        /// </summary>
        protected virtual bool DefaultFailCondition => Precision.AlmostBigger(Health.MinValue, Health.Value);

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
                        double lastTime = i > 0 ? healthIncreases[i - 1].time : gameplayStartTime;

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

            Health.Value = 1;
            HasFailed = false;
        }
    }
}
