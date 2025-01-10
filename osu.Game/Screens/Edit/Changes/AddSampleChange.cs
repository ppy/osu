// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Screens.Edit.Changes
{
    public class AddSampleChange : IRevertibleChange
    {
        public readonly IList<HitSampleInfo> Samples;

        public readonly HitSampleInfo Sample;

        public AddSampleChange(IList<HitSampleInfo> samples, HitSampleInfo sample)
        {
            Samples = samples;
            Sample = sample;
        }

        public void Apply() => Samples.Add(Sample);

        public void Revert() => Samples.Remove(Sample);
    }
}
