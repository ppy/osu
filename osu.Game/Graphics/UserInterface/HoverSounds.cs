// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Framework.Utils;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public class HoverSounds : HoverSampleDebounceComponent
    {
        private Sample sampleHover;

        protected readonly HoverSampleSet SampleSet;

        public HoverSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            SampleSet = sampleSet;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SessionStatics statics)
        {
            sampleHover = audio.Samples.Get($@"UI/generic-hover{SampleSet.GetDescription()}");
        }

        public override void PlayHoverSample()
        {
            sampleHover.Frequency.Value = 0.96 + RNG.NextDouble(0.08);
            sampleHover.Play();
        }
    }

    public enum HoverSampleSet
    {
        [Description("")]
        Loud,

        [Description("-soft")]
        Normal,

        [Description("-softer")]
        Soft,

        [Description("-toolbar")]
        Toolbar
    }
}
