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
    /// A <see cref="HealthProcessor"/> which adapts the impact of objects based on density.<br />
    /// At HP=0, you need at least 16 miss and at least 6 seconds to go from 100% to 0% health.<br />
    /// At HP=5, you need at least 8 miss and at least 3 seconds to go from 100% to 0% health.<br />
    /// At HP=10, you need at least 4 miss and at least 1.5 seconds to go from 100% to 0% health.<br />
    /// </summary>
    public partial class DynamicHealthProcessor : HealthProcessor
    {

        /// <summary>
        /// The beatmap.
        /// </summary>
        protected IBeatmap Beatmap { get; private set; }

        private double minimumMissesToDie;
        private double minimumMillisecondsToDie;

        /// <summary>
        /// How much misses does the great/ok/meh judgements compensate for
        /// </summary>
        private double greatIncrease;
        private double okIncrease;
        private double mehIncrease;
        private double missIncrease;

        private double previousObjectTime;

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            Beatmap = beatmap;

            minimumMissesToDie = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 16, 8, 4);
            minimumMillisecondsToDie = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 6000, 3000, 1500);

            greatIncrease = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 1, 0.5, 0.25);
            okIncrease = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0, 0, -0.05);
            mehIncrease = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0, -0.25, -0.5);
            missIncrease = -1;

            previousObjectTime = beatmap.HitObjects[0].StartTime;

            base.ApplyBeatmap(beatmap);
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            base.ApplyResultInternal(result);

            previousObjectTime = result.HitObject.GetEndTime();
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
        {
            double ratio = GetHealthIncreaseRatio(result.Type);
            double deltaTime = result.HitObject.GetEndTime() - previousObjectTime;
            if(deltaTime <= 0)
                return 0;

            return ratio / (minimumMissesToDie + minimumMillisecondsToDie / deltaTime);
        }

        private double GetHealthIncreaseRatio(HitResult result) {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.SmallTickHit:
                    return greatIncrease/4;

                case HitResult.SmallTickMiss:
                    return missIncrease/4;

                case HitResult.LargeTickHit:
                    return greatIncrease/2;

                case HitResult.LargeTickMiss:
                    return missIncrease/2;

                case HitResult.Miss:
                    return missIncrease;

                case HitResult.Meh:
                    return mehIncrease;

                case HitResult.Ok:
                    return okIncrease;

                case HitResult.Good:
                    return (okIncrease + greatIncrease)/2;

                case HitResult.Great:
                    return greatIncrease;

                case HitResult.Perfect:
                    return greatIncrease * 1.05;

                case HitResult.SmallBonus:
                    return greatIncrease * 0.5;

                case HitResult.LargeBonus:
                    return greatIncrease;
            }
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            previousObjectTime = Beatmap.HitObjects[0].StartTime; // Should I actually reset here ? Does reset mean restart map or is it used for restoring life ? (e.g EZ mod)
        }
    }
}
