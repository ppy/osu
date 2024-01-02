// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Spinner : OsuHitObject, IHasDuration
    {
        /// <summary>
        /// The RPM required to clear the spinner at ODs [ 0, 5, 10 ].
        /// </summary>
        private static readonly (int min, int mid, int max) clear_rpm_range = (90, 150, 225);

        /// <summary>
        /// The RPM required to complete the spinner and receive full score at ODs [ 0, 5, 10 ].
        /// </summary>
        private static readonly (int min, int mid, int max) complete_rpm_range = (250, 380, 430);

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
        /// The number of spins required to start receiving bonus score. The first bonus is awarded on this spin count.
        /// </summary>
        public int SpinsRequiredForBonus => SpinsRequired + bonus_spins_gap;

        /// <summary>
        /// The gap between spinner completion and the first bonus-awarding spin.
        /// </summary>
        private const int bonus_spins_gap = 2;

        /// <summary>
        /// The maximum RPM at which HP will be restored.
        /// </summary>
        private const int hp_ticks_per_minute = 500;

        /// <summary>
        /// Number of spins available to give bonus, beyond <see cref="SpinsRequired"/>.
        /// </summary>
        public int MaximumBonusSpins { get; protected set; } = 1;

        public override Vector2 StackOffset => Vector2.Zero;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            // The average RPS required over the length of the spinner to clear the spinner.
            double minRps = IBeatmapDifficultyInfo.DifficultyRange(difficulty.OverallDifficulty, clear_rpm_range) / 60;

            // The RPS required over the length of the spinner to receive full score (all normal + bonus ticks).
            double maxRps = IBeatmapDifficultyInfo.DifficultyRange(difficulty.OverallDifficulty, complete_rpm_range) / 60;

            double secondsDuration = Duration / 1000;

            // Allow a 0.1ms floating point precision error in the calculation of the duration.
            const double duration_error = 0.0001;

            SpinsRequired = (int)(minRps * secondsDuration + duration_error);
            MaximumBonusSpins = Math.Max(0, (int)(maxRps * secondsDuration + duration_error) - SpinsRequired - bonus_spins_gap);
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            int totalSpins = MaximumBonusSpins + SpinsRequired + bonus_spins_gap;

            for (int i = 0; i < totalSpins; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double startTime = StartTime + (float)(i + 1) / totalSpins * Duration;

                AddNested(i < SpinsRequiredForBonus
                    ? new SpinnerTick { StartTime = startTime, SpinnerDuration = Duration }
                    : new SpinnerBonusTick { StartTime = startTime, SpinnerDuration = Duration, Samples = new[] { CreateHitSampleInfo("spinnerbonus") } });
            }

            // Add ticks that give HP across the whole spinner duration
            int maxSpinsForHp = (int)(hp_ticks_per_minute * Duration / 60000);

            for (int i = 0; i < maxSpinsForHp; i++)
            {
                double startTime = StartTime + (float)(i + 1) / maxSpinsForHp * Duration;

                AddNested(new SpinnerHealthTick { StartTime = startTime, SpinnerDuration = Duration });
            }
        }

        public override Judgement CreateJudgement() => new OsuJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override IList<HitSampleInfo> AuxiliarySamples => CreateSpinningSamples();

        public HitSampleInfo[] CreateSpinningSamples()
        {
            var referenceSample = Samples.FirstOrDefault();

            if (referenceSample == null)
                return Array.Empty<HitSampleInfo>();

            return new[]
            {
                referenceSample.With("spinnerspin")
            };
        }
    }
}
