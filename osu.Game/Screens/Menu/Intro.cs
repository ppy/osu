// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using OpenTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class Intro : OsuScreen
    {
        private OsuLogo logo;

        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        internal bool DidLoadMenu;

        MainMenu mainMenu;
        private SampleChannel welcome;
        private SampleChannel seeya;
        private Track bgm;

        internal override bool ShowOverlays => false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenEmpty();

        public Intro()
        {
            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        logo = new OsuLogo
                        {
                            Alpha = 0,
                            Triangles = false,
                            BlendingMode = BlendingMode.Additive,
                            Interactive = false,
                            Colour = Color4.DarkGray,
                            Ripple = false
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            welcome = audio.Sample.Get(@"welcome");
            seeya = audio.Sample.Get(@"seeya");

            bgm = audio.Track.Get(@"circles");
            bgm.Looping = true;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            welcome.Play();

            Scheduler.AddDelayed(delegate
            {
                bgm.Start();

                (mainMenu = new MainMenu()).LoadAsync(Game);

                Scheduler.AddDelayed(delegate
                {
                    DidLoadMenu = true;
                    Push(mainMenu);
                }, 2300);
            }, 600);

            logo.ScaleTo(0.4f);
            logo.FadeOut();

            logo.ScaleTo(1, 4400, EasingTypes.OutQuint);
            logo.FadeIn(20000, EasingTypes.OutQuint);
        }

        protected override void OnSuspending(Screen next)
        {
            Content.FadeOut(300);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            //cancel exiting if we haven't loaded the menu yet.
            return !DidLoadMenu;
        }

        protected override void OnResuming(Screen last)
        {
            if (!(last is MainMenu))
                Content.FadeIn(300);

            //we also handle the exit transition.
            seeya.Play();

            double fadeOutTime = 2000;

            Scheduler.AddDelayed(Exit, fadeOutTime);

            //don't want to fade out completely else we will stop running updates and shit will hit the fan.
            Game.FadeTo(0.01f, fadeOutTime);

            base.OnResuming(last);
        }
    }
}
