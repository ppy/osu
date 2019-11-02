// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which requires pressing, holding, and releasing a key.
    /// </summary>
    public class HoldNote : ManiaHitObject, IHasEndTime
    {
        public double EndTime => StartTime + Duration;

        private double duration;

        public double Duration
        {
            get => duration;
            set
            {
                duration = value;
                Tail.StartTime = EndTime;
            }
        }

        public override double StartTime
        {
            get => base.StartTime;
            set
            {
                base.StartTime = value;
                Head.StartTime = value;
                Tail.StartTime = EndTime;
            }
        }

        public override int Column
        {
            get => base.Column;
            set
            {
                base.Column = value;
                Head.Column = value;
                Tail.Column = value;
            }
        }

        /// <summary>
        /// The head note of the hold.
        /// </summary>
        public readonly Note Head = new Note();

        /// <summary>
        /// The tail note of the hold.
        /// </summary>
        public readonly TailNote Tail = new TailNote();

        /// <summary>
        /// The time between ticks of this hold.
        /// </summary>
        private double tickSpacing = 50;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            tickSpacing = timingPoint.BeatLength / difficulty.SliderTickRate;
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            createTicks();

            AddNested(Head);
            AddNested(Tail);
        }

        private void createTicks()
        {
            if (tickSpacing == 0)
                return;

            for (double t = StartTime + tickSpacing; t <= EndTime - tickSpacing; t += tickSpacing)
            {
                AddNested(new HoldNoteTick
                {
                    StartTime = t,
                    Column = Column
                });
            }
        }

        public override Judgement CreateJudgement() => new HoldNoteJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
