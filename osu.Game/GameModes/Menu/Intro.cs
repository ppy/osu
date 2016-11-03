//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Threading;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.GameModes.Backgrounds;
using OpenTK.Graphics;
using osu.Framework;

namespace osu.Game.GameModes.Menu
{
    class Intro : OsuGameMode
    {
        private OsuLogo logo;

        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        internal bool DidLoadMenu;

        MainMenu mainMenu;
        private AudioSample welcome;
        private AudioTrack bgm;

        protected override BackgroundMode CreateBackground() => new BackgroundModeEmpty();

        public Intro()
        {
            Children = new Drawable[]
            {
                logo = new OsuLogo()
                {
                    Alpha = 0,
                    Additive = true,
                    Interactive = false,
                    Colour = Color4.DarkGray,
                    Ripple = false
                }
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);

            welcome = game.Audio.Sample.Get(@"welcome");

            bgm = game.Audio.Track.Get(@"circles");
            bgm.Looping = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.Add(delegate
            {
                welcome.Play();

                Scheduler.AddDelayed(delegate
                {
                    bgm.Start();

                    mainMenu = new MainMenu();
                    mainMenu.Preload(Game);

                    Scheduler.AddDelayed(delegate
                    {
                        DidLoadMenu = true;
                        Push(mainMenu);
                    }, 2300);
                }, 600);
            });

            logo.ScaleTo(0.4f);
            logo.FadeOut();

            logo.ScaleTo(1, 4400, EasingTypes.OutQuint);
            logo.FadeIn(20000, EasingTypes.OutQuint);
        }

        protected override void OnSuspending(GameMode next)
        {
            Content.FadeOut(300);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(GameMode next)
        {
            //cancel exiting if we haven't loaded the menu yet.
            return !DidLoadMenu;
        }

        protected override void OnResuming(GameMode last)
        {
            //we are just an intro. if we are resumed, we just want to exit after a short delay (to allow the last mode to transition out).
            Scheduler.AddDelayed(Exit, 600);

            base.OnResuming(last);
        }
    }
}
