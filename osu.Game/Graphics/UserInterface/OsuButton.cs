// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with added default sound effects.
    /// </summary>
    public class OsuButton : Button
    {
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        protected HoverSampleSet SampleSet = HoverSampleSet.Normal;

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

        public enum HoverSampleSet
        {
            [Description("")]
            Normal,
            [Description("-soft")]
            Soft,
            [Description("-softer")]
            Softer
        }
    }
}
