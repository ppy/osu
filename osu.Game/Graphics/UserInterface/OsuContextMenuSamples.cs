// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuContextMenuSamples : Component
    {
        private Sample sampleClick;
        private Sample sampleOpen;
        private Sample sampleClose;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select");
            sampleOpen = audio.Samples.Get(@"UI/dropdown-open");
            sampleClose = audio.Samples.Get(@"UI/dropdown-close");
        }

        public void PlayClickSample() => Scheduler.AddOnce(playClickSample);
        private void playClickSample() => sampleClick.Play();

        public void PlayOpenSample() => Scheduler.AddOnce(playOpenSample);
        private void playOpenSample() => sampleOpen.Play();

        public void PlayCloseSample() => Scheduler.AddOnce(playCloseSample);
        private void playCloseSample() => sampleClose.Play();
    }
}
