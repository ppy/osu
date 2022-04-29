// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;

namespace osu.Game.Graphics.UserInterface
{
    public class ShearedToggleButton : ShearedButton
    {
        private Sample? sampleOff;
        private Sample? sampleOn;

        /// <summary>
        /// Whether this button is currently toggled to an active state.
        /// </summary>
        public BindableBool Active { get; } = new BindableBool();

        /// <summary>
        /// Creates a new <see cref="ShearedToggleButton"/>
        /// </summary>
        /// <param name="width">
        /// The width of the button.
        /// <list type="bullet">
        /// <item>If a non-<see langword="null"/> value is provided, this button will have a fixed width equal to the provided value.</item>
        /// <item>If a <see langword="null"/> value is provided (or the argument is omitted entirely), the button will autosize in width to fit the text.</item>
        /// </list>
        /// </param>
        public ShearedToggleButton(float? width = null)
            : base(width)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds(sampleSet);

        protected override void LoadComplete()
        {
            Active.BindDisabledChanged(disabled => Action = disabled ? (Action?)null : Active.Toggle, true);
            Active.BindValueChanged(_ =>
            {
                updateActiveState();
                playSample();
            });

            updateActiveState();
            base.LoadComplete();
        }

        private void updateActiveState()
        {
            DarkerColour = Active.Value ? ColourProvider.Highlight1 : ColourProvider.Background3;
            LighterColour = Active.Value ? ColourProvider.Colour0 : ColourProvider.Background1;
            TextColour = Active.Value ? ColourProvider.Background6 : ColourProvider.Content1;
        }

        private void playSample()
        {
            if (Active.Value)
                sampleOn?.Play();
            else
                sampleOff?.Play();
        }
    }
}
