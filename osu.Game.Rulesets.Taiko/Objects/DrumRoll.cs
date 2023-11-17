// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;
using System.Collections.Generic;
using System;

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
        /// The length (in milliseconds) between ticks of this drumroll.
        /// <para>Half of this value is the hit window of the ticks.</para>
        /// </summary>
        private double tickSpacing = 100;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            EffectControlPoint effectPoint = controlPointInfo.EffectPointAt(StartTime);

            double scoringDistance = base_distance * difficulty.SliderMultiplier * effectPoint.ScrollSpeed;
            Velocity = scoringDistance / timingPoint.BeatLength;

            TickRate = difficulty.SliderTickRate == 3 ? 3 : 4;

            tickSpacing = timingPoint.BeatLength / TickRate;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            createTicks(cancellationToken);

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

                AddNested(new DrumRollTick(this)
                {
                    FirstTick = first,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong,
                    Samples = Samples
                });

                first = false;
            }
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not DrumRoll drumRoll)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(DrumRoll)}");

            Duration = drumRoll.Duration;
            Velocity = drumRoll.Velocity;
            TickRate = drumRoll.TickRate;
            tickSpacing = drumRoll.tickSpacing;
        }

        protected override HitObject CreateInstance() => new DrumRoll();

        public class StrongNestedHit : StrongNestedHitObject
        {
            // The strong hit of the drum roll doesn't actually provide any score.
            public override Judgement CreateJudgement() => new IgnoreJudgement();

            public StrongNestedHit(TaikoHitObject parent)
                : base(parent)
            {
            }

            protected override HitObject CreateInstance() => new StrongNestedHit(null!);
        }

        #region LegacyBeatmapEncoder

        double IHasDistance.Distance => Duration * Velocity;

        SliderPath IHasPath.Path
            => new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(1) }, ((IHasDistance)this).Distance / LegacyBeatmapEncoder.LEGACY_TAIKO_VELOCITY_MULTIPLIER);

        #endregion
    }
}
