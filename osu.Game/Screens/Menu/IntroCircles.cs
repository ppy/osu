// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Menu
{
    public class IntroCircles : IntroScreen
    {
        protected override string BeatmapHash => "3c8b1fcc9434dbb29e2fb613d3b9eada9d7bb6c125ceb32396c3b53437280c83";

        protected override string BeatmapFile => "circles.osz";

        private const double delay_step_one = 2300;
        private const double delay_step_two = 600;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (MenuVoice.Value)
                SetWelcome();
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                Beatmap.Value = IntroBeatmap;
                IntroBeatmap = null;

                Welcome?.Play();

                Scheduler.AddDelayed(delegate
                {
                    // Only start the current track if it is the menu music. A beatmap's track is started when entering the Main Menu.
                    if (MenuMusic.Value)
                    {
                        Track.Restart();
                        Track = null;
                    }

                    PrepareMenuLoad();

                    Scheduler.AddDelayed(LoadMenu, delay_step_one);
                }, delay_step_two);

                logo.ScaleTo(1);
                logo.FadeIn();
                logo.PlayIntro();
            }
        }

        public override void OnSuspending(IScreen next)
        {
            Track = null;

            this.FadeOut(300);
            base.OnSuspending(next);
        }
    }
}
