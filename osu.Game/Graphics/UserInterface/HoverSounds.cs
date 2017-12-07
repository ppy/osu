// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;

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

        protected override bool OnHover(InputState state)
        {
            // If Parent does not implement the interface, still play the sample
            if ((Parent as ICanDisableHoverSounds)?.ShouldPlayHoverSound != false) sampleHover?.Play();
            return base.OnHover(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get($@"UI/generic-hover{SampleSet.GetDescription()}");
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

    /// <summary>
    /// Classes implementing this interface can choose whether or not the HoverSounds should be played.
    /// <para>If this is not implemented, the sounds will always be played when OnHover is triggered.</para>
    /// </summary>
    public interface ICanDisableHoverSounds
    {
        bool ShouldPlayHoverSound { get; }
    }
}
