// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Threading;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public class HoverSounds : CompositeDrawable
    {
        private SampleChannel sampleHover;

        /// <summary>
        /// Length of debounce for hover sound playback, in milliseconds. Default is 50ms.
        /// </summary>
        public double HoverDebounceTime { get; } = 50;

        protected readonly HoverSampleSet SampleSet;

        public HoverSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            SampleSet = sampleSet;
            RelativeSizeAxes = Axes.Both;
        }

        private ScheduledDelegate playDelegate;

        protected override bool OnHover(HoverEvent e)
        {
            playDelegate?.Cancel();

            if (HoverDebounceTime <= 0)
                sampleHover?.Play();
            else
                playDelegate = Scheduler.AddDelayed(() => sampleHover?.Play(), HoverDebounceTime);

            return base.OnHover(e);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Samples.Get($@"UI/generic-hover{SampleSet.GetDescription()}");
        }
    }

    public enum HoverSampleSet
    {
        [Description("")]
        Loud,

        [Description("-soft")]
        Normal,

        [Description("-softer")]
        Soft
    }
}
