// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
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
        public HeadNote Head { get; protected set; }

        /// <summary>
        /// The tail note of the hold.
        /// </summary>
        public TailNote Tail { get; protected set; }

        /// <summary>
        /// The body of the hold.
        /// This is an invisible and silent object that tracks the holding state of the <see cref="HoldNote"/>.
        /// </summary>
        public HoldNoteBody Body { get; protected set; }

        public override double MaximumJudgementOffset => Tail.MaximumJudgementOffset;

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            // Generally node samples will be populated by ManiaBeatmapConverter, but in a case like the editor they may not be.
            // Ensure they are set to a sane default here.
            NodeSamples ??= CreateDefaultNodeSamples(this);

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
                Samples = GetNodeSamples(NodeSamples.Count - 1),
            });

            AddNested(Body = new HoldNoteBody
            {
                StartTime = StartTime,
                Column = Column
            });
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public IList<HitSampleInfo> GetNodeSamples(int nodeIndex) => nodeIndex < NodeSamples?.Count ? NodeSamples[nodeIndex] : Samples;

        /// <summary>
        /// Create the default note samples for a hold note, based off their main sample.
        /// </summary>
        /// <remarks>
        /// By default, osu!mania beatmaps in only play samples at the start of the hold note.
        /// </remarks>
        /// <param name="obj">The object to use as a basis for the head sample.</param>
        /// <returns>Defaults for assigning to <see cref="HoldNote.NodeSamples"/>.</returns>
        public static List<IList<HitSampleInfo>> CreateDefaultNodeSamples(HitObject obj) => new List<IList<HitSampleInfo>>
        {
            obj.Samples,
            new List<HitSampleInfo>(),
        };
    }
}
