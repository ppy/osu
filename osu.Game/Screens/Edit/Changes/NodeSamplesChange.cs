// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Changes
{
    public class NodeSamplesChange : PropertyChange<IHasRepeats, IList<HitSampleInfo>>
    {
        private readonly int nodeIndex;

        public NodeSamplesChange(IHasRepeats target, int nodeIndex, IList<HitSampleInfo> value)
            : base(target, value)
        {
            this.nodeIndex = nodeIndex;
        }

        protected override IList<HitSampleInfo> ReadValue(IHasRepeats target) => target.NodeSamples[nodeIndex];

        protected override void WriteValue(IHasRepeats target, IList<HitSampleInfo> value) => target.NodeSamples[nodeIndex] = value;
    }
}
