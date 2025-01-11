// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    public class SamplesChange : PropertyChange<HitObject, IList<HitSampleInfo>>
    {
        public SamplesChange(HitObject target, IList<HitSampleInfo> value)
            : base(target, value)
        {
        }

        // We have to copy the list because the Samples setter mutates the list instead of replacing the reference.
        protected override IList<HitSampleInfo> ReadValue(HitObject target) => target.Samples.ToList();

        protected override void WriteValue(HitObject target, IList<HitSampleInfo> value) => target.Samples = value;
    }
}
