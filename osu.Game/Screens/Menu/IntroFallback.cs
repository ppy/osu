// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Screens;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Menu
{
    public class IntroFallback : IntroScreen
    {
        protected override string BeatmapHash => "64E00D7022195959BFA3109D09C2E2276C8F12F486B91FCF6175583E973B48F2";
        protected override string BeatmapFile => "welcome.osz";
        private const double delay_step_two = 2142;

        private SampleChannel welcome;

        private SampleChannel pianoReverb;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            seeya = audio.Samples.Get(@"Intro/seeya-fallback");
            
            if (MenuVoice.Value)
            {
                welcome = audio.Samples.Get(@"Intro/welcome-fallback");
                pianoReverb = audio.Samples.Get(@"Intro/welcome_piano");
            }
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                welcome?.Play();
                pianoReverb?.Play();
                Scheduler.AddDelayed(delegate
                {
                    StartTrack();

                    PrepareMenuLoad();

                    Scheduler.Add(LoadMenu);
                    
                }, delay_step_two);

                logo.ScaleTo(1);
                logo.FadeIn();
                logo.PlayIntro();
            }
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(300);
            base.OnSuspending(next);
        }
    }
}
