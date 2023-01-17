// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Threading;

namespace osu.Game.Screens.Edit.Timing
{
    /// <summary>
    /// Represents a component that provides the behaviour of triggering button clicks repeatedly while holding with mouse.
    /// </summary>
    public partial class RepeatingButtonBehaviour : Component
    {
        private const double initial_delay = 300;
        private const double minimum_delay = 80;

        private readonly Drawable button;

        private Sample? sample;

        public Action? RepeatBegan;
        public Action? RepeatEnded;

        /// <summary>
        /// An additive modifier for the frequency of the sample played on next actuation.
        /// This can be adjusted during the button's <see cref="Drawable.OnClick"/> event to affect the repeat sample playback of that click.
        /// </summary>
        public double SampleFrequencyModifier { get; set; }

        public RepeatingButtonBehaviour(Drawable button)
        {
            this.button = button;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sample = audio.Samples.Get(@"UI/notch-tick");
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            RepeatBegan?.Invoke();
            beginRepeat();
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            adjustDelegate?.Cancel();
            RepeatEnded?.Invoke();
            base.OnMouseUp(e);
        }

        private ScheduledDelegate? adjustDelegate;
        private double adjustDelay = initial_delay;

        private void beginRepeat()
        {
            adjustDelegate?.Cancel();

            adjustDelay = initial_delay;
            adjustNext();

            void adjustNext()
            {
                if (IsHovered)
                {
                    button.TriggerClick();
                    adjustDelay = Math.Max(minimum_delay, adjustDelay * 0.9f);

                    var channel = sample?.GetChannel();

                    if (channel != null)
                    {
                        double repeatModifier = 0.05f * (Math.Abs(adjustDelay - initial_delay) / minimum_delay);
                        channel.Frequency.Value = 1 + repeatModifier + SampleFrequencyModifier;
                        channel.Play();
                    }
                }
                else
                {
                    adjustDelay = initial_delay;
                }

                adjustDelegate = Scheduler.AddDelayed(adjustNext, adjustDelay);
            }
        }
    }
}
