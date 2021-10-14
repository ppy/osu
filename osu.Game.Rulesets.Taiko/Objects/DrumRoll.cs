// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using System;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class DrumRoll : TaikoStrongableHitObject, IHasPath
    {
        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float base_distance = 100;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        /// <summary>
        /// Velocity of this <see cref="DrumRoll"/>.
        /// </summary>
        public double Velocity { get; private set; }

        /// <summary>
        /// Numer of ticks per beat length.
        /// </summary>
        public int TickRate = 1;

        /// <summary>
        /// Number of drum roll ticks required for a "Good" hit.
        /// </summary>
        public double RequiredGoodHits { get; protected set; }

        /// <summary>
        /// Number of drum roll ticks required for a "Great" hit.
        /// </summary>
        public double RequiredGreatHits { get; protected set; }

        /// <summary>
        /// The length (in milliseconds) between ticks of this drumroll.
        /// <para>Half of this value is the hit window of the ticks.</para>
        /// </summary>
        private double tickSpacing = 100;

        private float overallDifficulty = BeatmapDifficulty.DEFAULT_DIFFICULTY;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);

            double scoringDistance = base_distance * difficulty.SliderMultiplier * DifficultyControlPoint.SliderVelocity;
            Velocity = scoringDistance / timingPoint.BeatLength;

            tickSpacing = timingPoint.BeatLength / TickRate;
            overallDifficulty = difficulty.OverallDifficulty;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            createTicks(cancellationToken);

            RequiredGoodHits = NestedHitObjects.Count * Math.Min(0.15, 0.05 + 0.10 / 6 * overallDifficulty);
            RequiredGreatHits = NestedHitObjects.Count * Math.Min(0.30, 0.10 + 0.20 / 6 * overallDifficulty);

            base.CreateNestedHitObjects(cancellationToken);
        }

        private void createTicks(CancellationToken cancellationToken)
        {
            if (tickSpacing == 0)
                return;

            bool first = true;

            for (double t = StartTime; t < EndTime + tickSpacing / 2; t += tickSpacing)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddNested(new DrumRollTick
                {
                    FirstTick = first,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong
                });

                first = false;
            }
        }

        public override Judgement CreateJudgement() => new TaikoDrumRollJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit { StartTime = startTime };

        public class StrongNestedHit : StrongNestedHitObject
        {
        }

        #region LegacyBeatmapEncoder

        double IHasDistance.Distance => Duration * Velocity;

        SliderPath IHasPath.Path
            => new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(1) }, ((IHasDistance)this).Distance / LegacyBeatmapEncoder.LEGACY_TAIKO_VELOCITY_MULTIPLIER);

        #endregion
    }
}
