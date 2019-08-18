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

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Adds hover sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public class HoverSounds : CompositeDrawable
    {
        private SampleChannel sampleHover;

        protected readonly HoverSampleSet SampleSet;

        public HoverSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            SampleSet = sampleSet;
            RelativeSizeAxes = Axes.Both;
        }

        protected override bool OnHover(HoverEvent e)
        {
            sampleHover?.Play();
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
