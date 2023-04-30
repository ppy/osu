// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Game.Rulesets.Objects.Types;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class DrumRoll : TaikoStrongableHitObject, IHasPath, IHasSliderVelocity
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

        public BindableNumber<double> SliderVelocityBindable { get; } = new BindableDouble(1)
        {
            Precision = 0.01,
            MinValue = 0.1,
            MaxValue = 10
        };

        public double SliderVelocity
        {
            get => SliderVelocityBindable.Value;
            set => SliderVelocityBindable.Value = value;
        }

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

            double scoringDistance = base_distance * difficulty.SliderMultiplier * SliderVelocity;
            Velocity = scoringDistance / timingPoint.BeatLength;

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

                AddNested(new DrumRollTick
                {
                    FirstTick = first,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong,
                    Samples = Samples.Where(s => s.Name == HitSampleInfo.HIT_FINISH).ToList()
                });

                first = false;
            }
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit { StartTime = startTime };

        public class StrongNestedHit : StrongNestedHitObject
        {
            // The strong hit of the drum roll doesn't actually provide any score.
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }

        #region LegacyBeatmapEncoder

        double IHasDistance.Distance => Duration * Velocity;

        SliderPath IHasPath.Path
            => new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(1) }, ((IHasDistance)this).Distance / LegacyBeatmapEncoder.LEGACY_TAIKO_VELOCITY_MULTIPLIER);

        #endregion
    }
}
