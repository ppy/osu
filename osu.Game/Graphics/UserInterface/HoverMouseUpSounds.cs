// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover sounds to a drawable, as well as click sounds upon MouseUp events for selected mouse buttons.
    /// Intended to be used for controls that can respond to clicks of buttons other than the left mouse button in place of <see cref="HoverClickSounds" />.
    /// </summary>
    public class HoverMouseUpSounds : HoverSounds
    {
        private SampleChannel sampleClick;
        private readonly List<MouseButton> buttons;

        public HoverMouseUpSounds(List<MouseButton> buttons, HoverSampleSet sampleSet = HoverSampleSet.Normal)
            : base(sampleSet)
        {
            this.buttons = buttons;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            if (Contains(e.ScreenSpaceMousePosition) && buttons.Contains(e.Button))
                sampleClick?.Play();
            return base.OnMouseUp(e);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Samples.Get($@"UI/generic-select{SampleSet.GetDescription()}");
        }
    }
}
