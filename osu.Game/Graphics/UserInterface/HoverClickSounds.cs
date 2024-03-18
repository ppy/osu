// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover and click sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public partial class HoverClickSounds : HoverSounds
    {
        public Bindable<bool> Enabled = new Bindable<bool>(true);

        private Sample sampleClick;
        private Sample sampleClickDisabled;

        private readonly MouseButton[] buttons;

        /// <summary>
        /// a container which plays sounds on hover and click for any specified <see cref="MouseButton"/>s.
        /// </summary>
        /// <param name="sampleSet">Set of click samples to play.</param>
        /// <param name="buttons">
        /// Array of button codes which should trigger the click sound.
        /// If this optional parameter is omitted or set to <code>null</code>, the click sound will only be played on left click.
        /// </param>
        public HoverClickSounds(HoverSampleSet sampleSet = HoverSampleSet.Default, MouseButton[] buttons = null)
            : base(sampleSet)
        {
            this.buttons = buttons ?? new[] { MouseButton.Left };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Samples.Get($@"UI/{SampleSet.GetDescription()}-select")
                          ?? audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select");

            sampleClickDisabled = audio.Samples.Get($@"UI/{SampleSet.GetDescription()}-select-disabled")
                                  ?? audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select-disabled");
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (buttons.Contains(e.Button))
                PlayAsClick(Enabled.Value ? sampleClick : sampleClickDisabled);

            return base.OnClick(e);
        }

        public override void PlayHoverSample()
        {
            if (!Enabled.Value)
                return;

            base.PlayHoverSample();
        }

        public static void PlayAsClick(Sample sample)
        {
            var channel = sample?.GetChannel();

            if (channel != null)
            {
                channel.Frequency.Value = 0.99 + RNG.NextDouble(0.02);
                channel.Play();
            }
        }
    }
}
