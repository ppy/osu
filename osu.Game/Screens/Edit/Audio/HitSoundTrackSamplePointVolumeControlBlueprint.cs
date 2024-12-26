// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Extensions;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackSamplePointVolumeControlBlueprint : HitSoundTrackSamplePointBlueprint
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        protected BindableInt Volume = new BindableInt
        {
            Default = 50,
            MinValue = 0,
            MaxValue = 100,
        };

        public HitSoundTrackSamplePointVolumeControlBlueprint(HitObject hitObject, IList<HitSampleInfo> samples)
            : base(hitObject, samples)
        {
            BindVolumeToSample();
        }

        protected virtual void BindVolumeToSample()
        {
            HitObject.SamplesBindable.BindCollectionChanged((obj, v) =>
            {
                Volume.Value = SamplePointPiece.GetVolumeValue(HitObject.Samples);
            }, true);
        }

        private void commitVolumeChange(int volume)
        {
            editorBeatmap.BeginChange();

            IList<HitSampleInfo> newSamples = [];

            Samples.ForEach(sample =>
            {
                newSamples.Add(sample.With(newVolume: volume));
            });

            Samples.Clear();
            Samples.AddRange(newSamples);

            editorBeatmap.Update(HitObject);
            editorBeatmap.EndChange();
        }

        protected override IReadOnlyList<Drawable> CreateControls()
        {
            return new Drawable[]
            {
                new HitSoundTrackSamplePointVolumeControl
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Current = Volume,
                    OnVolumeChange = commitVolumeChange,
                }
            };
        }
    }

    public partial class NodeHitSoundTrackSamplePointVolumeControlBlueprint : HitSoundTrackSamplePointVolumeControlBlueprint, INodeHitSoundTrackSamplePointBlueprint
    {
        public IHasRepeats HasRepeat { get; }

        public int NodeIndex { get; }

        public NodeHitSoundTrackSamplePointVolumeControlBlueprint(HitObject hitObject, IList<HitSampleInfo> samples, int nodeIndex)
            : base(hitObject, samples)
        {
            if (hitObject is not IHasRepeats hasRepeat)
                throw new ArgumentException("HitObject must be implement IHasRepeats");

            HasRepeat = hasRepeat;
            NodeIndex = nodeIndex;
            Samples = HasRepeat.NodeSamples[NodeIndex];
        }

        //No bindable available in node sample, needs to be monitor volume updates in Update()
        protected override void BindVolumeToSample() { }

        protected override void Update()
        {
            base.Update();

            if (HasRepeat.RepeatCount < NodeIndex - 1)
                Expire();

            int volume = SamplePointPiece.GetVolumeValue(Samples);
            if (Volume.Value != volume)
                Volume.Value = volume;
        }

        protected override double GetStartTime() => HitObject.StartTime + HasRepeat.Duration * NodeIndex / HasRepeat.SpanCount();

        protected override double GetWidth() => CIRCLE_WIDTH;
    }
}
