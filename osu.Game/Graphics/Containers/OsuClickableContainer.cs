// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;

namespace osu.Game.Graphics.Containers
{
    public class OsuClickableContainer : ClickableContainer
    {
        protected SampleChannel SampleClick, SampleHover;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            SampleHover = audio.Sample.Get(@"UI/generic-hover");
            SampleClick = audio.Sample.Get(@"UI/generic-click");
        }

        protected override bool OnHover(InputState state)
        {
            SampleHover?.Play();
            return base.OnHover(state);
        }

        protected override bool OnClick(InputState state)
        {
            SampleClick?.Play();
            return base.OnClick(state);
        }
    }
}
