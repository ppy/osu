// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover and click sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public class HoverClickSounds : HoverSounds
    {
        private SampleChannel sampleClick;
        private readonly MouseButton[] buttons;

        /// <summary>
        /// a container which plays sounds on hover and click for any specified <see cref="MouseButton"/>s.
        /// </summary>
        /// <param name="sampleSet">Set of click samples to play.</param>
        /// <param name="buttons">
        /// Array of button codes which should trigger the click sound.
        /// If this optional parameter is omitted or set to <code>null</code>, the click sound will only be played on left click.
        /// </param>
        public HoverClickSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal, MouseButton[] buttons = null)
            : base(sampleSet)
        {
            this.buttons = buttons ?? new[] { MouseButton.Left };
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (buttons.Contains(e.Button) && Contains(e.ScreenSpaceMousePosition))
                sampleClick?.Play();

            return base.OnClick(e);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Samples.Get($@"UI/generic-select{SampleSet.GetDescription()}");
        }
    }
}
