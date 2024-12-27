// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class NodeHitSoundTrackSamplePointVolumeControlBlueprint : HitSoundTrackSamplePointVolumeControlBlueprint, INodeHitSoundTrackSamplePointBlueprint
    {
        public IHasRepeats HasRepeat { get; }

        public int NodeIndex { get; }

        protected override double GetStartTime() => HitObject.StartTime + (HasRepeat.Duration * NodeIndex / HasRepeat.SpanCount());

        protected override double GetWidth() => CIRCLE_WIDTH;

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
    }
}
