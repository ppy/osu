// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;

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
        /// Length of debounce for hover sound playback, in milliseconds.
        /// </summary>
        public double HoverDebounceTime { get; } = 20;

        protected readonly HoverSampleSet SampleSet;

        private Bindable<double?> lastPlaybackTime;

        public HoverSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            SampleSet = sampleSet;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SessionStatics statics)
        {
            lastPlaybackTime = statics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime);

            sampleHover = audio.Samples.Get($@"UI/generic-hover{SampleSet.GetDescription()}");
        }

        protected override bool OnHover(HoverEvent e)
        {
            bool enoughTimePassedSinceLastPlayback = !lastPlaybackTime.Value.HasValue || Time.Current - lastPlaybackTime.Value >= HoverDebounceTime;

            if (enoughTimePassedSinceLastPlayback)
            {
                sampleHover?.Play();
                lastPlaybackTime.Value = Time.Current;
            }

            return base.OnHover(e);
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
