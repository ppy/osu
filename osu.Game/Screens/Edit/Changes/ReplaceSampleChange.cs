// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Screens.Edit.Changes
{
    public class ReplaceSampleChange : PropertyChange<IList<HitSampleInfo>, HitSampleInfo>
    {
        public readonly int Index;

        public ReplaceSampleChange(IList<HitSampleInfo> target, int index, HitSampleInfo value)
            : base(target, value)
        {
            Index = index;
        }

        protected override HitSampleInfo ReadValue(IList<HitSampleInfo> target) => target[Index];

        protected override void WriteValue(IList<HitSampleInfo> target, HitSampleInfo value) => target[Index] = value;
    }
}
