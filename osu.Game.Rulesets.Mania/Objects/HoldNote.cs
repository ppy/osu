// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which requires pressing, holding, and releasing a key.
    /// </summary>
    public class HoldNote : ManiaHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        private double duration;

        public double Duration
        {
            get => duration;
            set
            {
                duration = value;

                if (Tail != null)
                    Tail.StartTime = EndTime;
            }
        }

        public override double StartTime
        {
            get => base.StartTime;
            set
            {
                base.StartTime = value;

                if (Head != null)
                    Head.StartTime = value;

                if (Tail != null)
                    Tail.StartTime = EndTime;
            }
        }

        public override int Column
        {
            get => base.Column;
            set
            {
                base.Column = value;

                if (Head != null)
                    Head.Column = value;

                if (Tail != null)
                    Tail.Column = value;
            }
        }

        public IList<IList<HitSampleInfo>> NodeSamples { get; set; }

        /// <summary>
        /// The head note of the hold.
        /// </summary>
        public HeadNote Head { get; private set; }

        /// <summary>
        /// The tail note of the hold.
        /// </summary>
        public TailNote Tail { get; private set; }

        /// <summary>
        /// The time between ticks of this hold.
        /// </summary>
        private double tickSpacing = 50;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            tickSpacing = timingPoint.BeatLength / difficulty.SliderTickRate;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            createTicks(cancellationToken);

            AddNested(Head = new HeadNote
            {
                StartTime = StartTime,
                Column = Column,
                Samples = GetNodeSamples(0),
            });

            AddNested(Tail = new TailNote
            {
                StartTime = EndTime,
                Column = Column,
                Samples = GetNodeSamples((NodeSamples?.Count - 1) ?? 1),
            });
        }

        private void createTicks(CancellationToken cancellationToken)
        {
            if (tickSpacing == 0)
                return;

            for (double t = StartTime + tickSpacing; t <= EndTime - tickSpacing; t += tickSpacing)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddNested(new HoldNoteTick
                {
                    StartTime = t,
                    Column = Column
                });
            }
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public IList<HitSampleInfo> GetNodeSamples(int nodeIndex) =>
            nodeIndex < NodeSamples?.Count ? NodeSamples[nodeIndex] : Samples;
    }
}
