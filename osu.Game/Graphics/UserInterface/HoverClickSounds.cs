// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover and click sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public class HoverClickSounds : HoverSounds
    {
        private SampleChannel sampleClick;

        public HoverClickSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal)
            : base(sampleSet)
        {
        }

        protected override bool OnClick(ClickEvent e)
        {
            sampleClick?.Play();
            return base.OnClick(e);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get($@"UI/generic-select{SampleSet.GetDescription()}");
        }
    }
}
