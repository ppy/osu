// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class OsuMenuSamples : Component
    {
        private Sample sampleClick;
        private Sample sampleOpen;
        private Sample sampleSubOpen;
        private Sample sampleClose;

        private bool triggerOpen;
        private bool triggerSubOpen;
        private bool triggerClose;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Samples.Get(@"UI/menu-open-select");
            sampleOpen = audio.Samples.Get(@"UI/menu-open");
            sampleSubOpen = audio.Samples.Get(@"UI/menu-sub-open");
            sampleClose = audio.Samples.Get(@"UI/menu-close");
        }

        public void PlayClickSample()
        {
            Scheduler.AddOnce(playClickSample);
        }

        public void PlayOpenSample()
        {
            triggerOpen = true;
            Scheduler.AddOnce(resolvePlayback);
        }

        public void PlaySubOpenSample()
        {
            triggerSubOpen = true;
            Scheduler.AddOnce(resolvePlayback);
        }

        public void PlayCloseSample()
        {
            triggerClose = true;
            Scheduler.AddOnce(resolvePlayback);
        }

        private void playClickSample() => sampleClick.Play();

        private void resolvePlayback()
        {
            if (triggerSubOpen)
                sampleSubOpen?.Play();
            else if (triggerOpen)
                sampleOpen?.Play();
            else if (triggerClose)
                sampleClose?.Play();

            triggerOpen = triggerSubOpen = triggerClose = false;
        }
    }
}
