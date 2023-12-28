// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A <see cref="HealthProcessor"/> which adapts the drain and impact of objects based on density.<br />
    /// At HP=0, you need at least 16 miss and at least 4 seconds to go from 100% to 0% health.<br />
    /// At HP=5, you need at least 8 miss and at least 2 seconds to go from 100% to 0% health.<br />
    /// At HP=10, you need at least 4 miss and at least 1 seconds to go from 100% to 0% health.<br />
    /// </summary>
    public partial class DynamicHealthProcessor : HealthProcessor
    {
        /// <summary>
        /// The beatmap.
        /// </summary>
        protected IBeatmap Beatmap { get; private set; }

        /// <summary>
        /// These values determine the asymptotic cost of objects/drain in sparse sections.
        /// </summary>
        private double maxDrainPerObject;
        private double maxHPcostPerMiss;

        /// <summary>
        /// These values determine the asymptotic health cost of drain/misses in dense section.
        /// It is defined in hp per milliseconds.
        /// </summary>
        private double maxDrainRate;
        private double maxMissCostRate;

        /// <summary>
        /// How much misses does great/ok judgements compensate for
        /// </summary>
        private double greatIncrease;
        private double okIncrease;
        private readonly double missIncrease = -1;

        /// <summary>
        /// How much of the preceding drain does great/ok judgements compensate for
        /// </summary>
        private double greatDrainCompensation;
        private double okDrainCompensation;
        private readonly double missDrainCompensation = 0;

        /// <summary>
        /// The time at which health starts and ends draining.
        /// </summary>
        private readonly double drainStartTime;
        private double drainEndTime;

        /// <summary>
        /// For each given <see cref="HitObject"/> in the map, this dictionary maps the object onto the latest end time of any other object
        /// that precedes the end time of the given object.
        /// This can be loosely interpreted as the end time of the preceding hit object in rulesets that do not have overlapping hit objects.
        /// </summary>
        private readonly Dictionary<HitObject, double> precedingEndTimes = new Dictionary<HitObject, double>();

        private double currentGameplayTime;
        private double previousObjectTime;
        private double previousObjectHealth;

        public DynamicHealthProcessor(double drainStartTime = 0)
        {
            this.drainStartTime = drainStartTime;
        }

        protected override void Update()
        {
            base.Update();

            currentGameplayTime = Math.Clamp(Time.Current, drainStartTime, drainEndTime);
            updateHealthDrain();
        }

        private void updateHealthDrain()
        {
            double timeDelta = currentGameplayTime - previousObjectTime;

            double drain = dynamicValue(timeDelta, maxDrainRate, maxDrainPerObject);

            Health.Value = previousObjectHealth - drain;
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            Beatmap = beatmap;
            double precedingEndTime = 0;
            if (beatmap.HitObjects.Count > 0)
            {
                drainEndTime = beatmap.HitObjects[^1].GetEndTime();
                previousObjectTime = beatmap.HitObjects[0].StartTime;
                precedingEndTime = previousObjectTime;
            }

            maxDrainPerObject = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.1, 0.2, 0.25);
            maxDrainRate = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 1.0 / 12000, 1.0 / 9000, 1.0 / 5000);

            maxHPcostPerMiss = 0.025;
            maxMissCostRate = 1.0 / 4000;

            greatIncrease = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.7, 0.6, 0.4);
            greatDrainCompensation = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 1.1, 1.04, 1.02);

            okIncrease = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, -0.2, -0.25, -0.3);
            okDrainCompensation = 0.35;

            precedingEndTimes.Clear();
            foreach (HitObject hitObject in beatmap.HitObjects)
            {
                foreach (HitObject nested in hitObject.NestedHitObjects)
                {
                    precedingEndTimes.Add(nested, precedingEndTime);
                    precedingEndTime = nested.GetEndTime();
                }
                precedingEndTimes.Add(hitObject, precedingEndTime);
                precedingEndTime = hitObject.GetEndTime();
            }

            base.ApplyBeatmap(beatmap);
        }

        protected override bool DefaultFailCondition => 0 >= previousObjectHealth;

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = previousObjectHealth;
            result.FailedAtJudgement = HasFailed;

            if (HasFailed)
                return;

            previousObjectHealth += GetHealthIncreaseFor(result);
            previousObjectHealth = Math.Min(previousObjectHealth, 1);
            previousObjectTime = result.HitObject.GetEndTime();
            updateHealthDrain();

            if (MeetsAnyFailCondition(result))
                TriggerFailure();
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            previousObjectHealth = result.HealthAtJudgement;
            previousObjectTime = precedingEndTimes[result.HitObject];
            updateHealthDrain();

            // Todo: (As in HealthProcessor) Revert HasFailed state with proper player support
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
        {
            double objectValue = getHealthObjectValue(result.Type);
            double timeDelta = result.HitObject.GetEndTime() - previousObjectTime;
            if (timeDelta <= 0)
                return 0;

            double dynamicDrain = dynamicValue(timeDelta, maxDrainRate, maxDrainPerObject);
            double drain = dynamicDrain * (1 - getDrainCompensation(result.Type));

            double dynamicCost = dynamicValue(timeDelta, maxMissCostRate, maxHPcostPerMiss * objectValue);
            double gain = dynamicCost * getHealthIncreaseRatio(result.Type);

            return gain - drain;
        }

        /// <summary>
        /// Computes a dynamic value that increases based on a time delta.
        /// It is defined by its maximum growth rate and maximum value.
        /// </summary>
        /// <param name="t">time delta for which the function is evaluated</param>
        /// <param name="maxRate">the max rate at which the value can grow</param>
        /// <param name="maxCost">the max value that can be reached</param>
        /// <returns></returns>
        private double dynamicValue(double t, double maxRate, double maxCost)
        {
            if (t <= 0)
                return 0;
            return maxCost * (1 - Math.Exp(-t * maxRate / maxCost));
        }

        private double getHealthObjectValue(HitResult result)
        {
            switch (result)
            {
                case HitResult.SmallTickHit:
                case HitResult.SmallTickMiss:
                    return 0.25;

                case HitResult.LargeTickHit:
                case HitResult.LargeTickMiss:
                case HitResult.SmallBonus:
                    return 0.5;

                default:
                    return 1;
            }
        }

        private double getHealthIncreaseRatio(HitResult result)
        {
            return getInterpolatedValue(result, greatIncrease, okIncrease, missIncrease);
        }

        private double getDrainCompensation(HitResult result)
        {
            return getInterpolatedValue(result, greatDrainCompensation, okDrainCompensation, missDrainCompensation);
        }

        private double getInterpolatedValue(HitResult result, double greatValue, double okValue, double missValue)
        {
            switch (result)
            {
                case HitResult.SmallTickMiss:
                case HitResult.LargeTickMiss:
                case HitResult.Miss:
                    return missValue;

                case HitResult.Meh:
                    return (okValue + missValue) / 2;

                case HitResult.Ok:
                    return okValue;

                case HitResult.Good:
                    return (okValue + greatValue) / 2;

                case HitResult.Perfect:
                    return greatIncrease * 1.05;

                default:
                    return greatValue;
            }
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            previousObjectTime = Beatmap.HitObjects[0].StartTime;
            previousObjectHealth = 1;
        }
    }
}
