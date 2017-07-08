// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public class OsuFocusedOverlayContainer : FocusedOverlayContainer
    {
        private SampleChannel samplePopIn;
        private SampleChannel samplePopOut;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            samplePopIn = audio.Sample.Get(@"UI/melodic-5");
            samplePopOut = audio.Sample.Get(@"UI/melodic-4");

            StateChanged += OsuFocusedOverlayContainer_StateChanged;
        }

        private void OsuFocusedOverlayContainer_StateChanged(VisibilityContainer arg1, Visibility arg2)
        {
            switch (arg2)
            {
                case Visibility.Visible:
                    samplePopIn?.Play();
                    break;
                case Visibility.Hidden:
                    samplePopOut?.Play();
                    break;
            }
        }
    }
}
