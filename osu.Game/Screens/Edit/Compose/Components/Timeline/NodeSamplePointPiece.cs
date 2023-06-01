// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class NodeSamplePointPiece : SamplePointPiece
    {
        public readonly int NodeIndex;

        public NodeSamplePointPiece(HitObject hitObject, int nodeIndex)
            : base(hitObject)
        {
            if (hitObject is not IHasRepeats)
                throw new System.ArgumentException($"HitObject must implement {nameof(IHasRepeats)}", nameof(hitObject));

            NodeIndex = nodeIndex;
        }

        protected override IList<HitSampleInfo> GetSamples()
        {
            var hasRepeats = (IHasRepeats)HitObject;
            return NodeIndex < hasRepeats.NodeSamples.Count ? hasRepeats.NodeSamples[NodeIndex] : HitObject.Samples;
        }

        public override Popover GetPopover() => new NodeSampleEditPopover(HitObject, NodeIndex);

        public partial class NodeSampleEditPopover : SampleEditPopover
        {
            private readonly int nodeIndex;

            protected override IList<HitSampleInfo> GetRelevantSamples(HitObject ho)
            {
                if (ho is not IHasRepeats hasRepeats)
                    return ho.Samples;

                return nodeIndex < hasRepeats.NodeSamples.Count ? hasRepeats.NodeSamples[nodeIndex] : ho.Samples;
            }

            public NodeSampleEditPopover(HitObject hitObject, int nodeIndex)
                : base(hitObject)
            {
                this.nodeIndex = nodeIndex;
            }
        }
    }
}
