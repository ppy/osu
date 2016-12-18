//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.GameModes;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics.Containers;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Charts;
using osu.Game.Screens.Direct;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Multiplayer;
using osu.Game.Screens.Play;
using OpenTK;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Menu
{
    public class MainMenu : OsuGameMode
    {
        private ButtonSystem buttons;
        public override string Name => @"Main Menu";

        internal override bool ShowOverlays => true;

        private BackgroundMode background;

        protected override BackgroundMode CreateBackground() => background;

        public MainMenu()
        {
            background = new BackgroundModeDefault();

            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem()
                        {
                            OnChart = delegate { Push(new ChartListing()); },
                            OnDirect = delegate { Push(new OnlineListing()); },
                            OnEdit = delegate { Push(new EditSongSelect()); },
                            OnSolo = delegate { Push(new PlaySongSelect()); },
                            OnMulti = delegate { Push(new Lobby()); },
                            OnTest  = delegate { Push(new TestBrowser()); },
                            OnExit = delegate { Exit(); },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame game)
        {
            background.Preload(game);

            buttons.OnSettings = game.ToggleOptions;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            buttons.FadeInFromZero(500);
        }

        protected override void OnSuspending(GameMode next)
        {
            base.OnSuspending(next);

            const float length = 400;

            buttons.State = MenuState.EnteringMode;

            Content.FadeOut(length, EasingTypes.InSine);
            Content.MoveTo(new Vector2(-800, 0), length, EasingTypes.InSine);
        }

        protected override void OnResuming(GameMode last)
        {
            base.OnResuming(last);

            const float length = 300;

            buttons.State = MenuState.TopLevel;

            Content.FadeIn(length, EasingTypes.OutQuint);
            Content.MoveTo(new Vector2(0, 0), length, EasingTypes.OutQuint);
        }

        protected override bool OnExiting(GameMode next)
        {
            buttons.State = MenuState.Exit;
            Content.FadeOut(ButtonSystem.EXIT_DELAY);
            return base.OnExiting(next);
        }
    }
}
