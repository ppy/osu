// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Spinner : OsuHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        /// <summary>
        /// Number of spins required to finish the spinner without miss.
        /// </summary>
        public int SpinsRequired { get; protected set; } = 1;

        /// <summary>
        /// Number of spins available to give bonus, beyond <see cref="SpinsRequired"/>.
        /// </summary>
        public int MaximumBonusSpins => (int)(SpinsRequired * 1.8f); // roughly matches stable

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            SpinsRequired = (int)(Duration / 1000 * BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 3, 5, 7.5));

            // spinning doesn't match 1:1 with stable, so let's fudge them easier for the time being.
            SpinsRequired = (int)Math.Max(1, SpinsRequired * 0.6);
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            int totalSpins = MaximumBonusSpins + SpinsRequired;

            for (int i = 0; i < totalSpins; i++)
            {
                double startTime = StartTime + (float)(i + 1) / totalSpins * Duration;

                AddNested(i < SpinsRequired
                    ? new SpinnerTick { StartTime = startTime }
                    : new SpinnerBonusTick { StartTime = startTime });
            }
        }

        public override Judgement CreateJudgement() => new OsuJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
