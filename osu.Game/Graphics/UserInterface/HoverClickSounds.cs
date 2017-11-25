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
    /// Adds hover and click sounds to a drawable.
    /// Does not draw anything.
    /// </summary>
    public class HoverClickSounds : CompositeDrawable
    {
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        protected readonly HoverSampleSet SampleSet;

        public HoverClickSounds(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            SampleSet = sampleSet;
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        protected override bool OnClick(InputState state)
        {
            sampleClick?.Play();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            sampleHover?.Play();
            return base.OnHover(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get($@"UI/generic-select{SampleSet.GetDescription()}");
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
}
