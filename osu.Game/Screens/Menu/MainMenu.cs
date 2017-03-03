// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Charts;
using osu.Game.Screens.Direct;
using osu.Game.Screens.Multiplayer;
using OpenTK;
using osu.Game.Screens.Select;
using osu.Game.Screens.Tournament;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Screens.Menu
{
    public class MainMenu : OsuScreen
    {
        private ButtonSystem buttons;
        public override string Name => @"Main Menu";

        internal override bool ShowOverlays => buttons.State != MenuState.Initial;

        private BackgroundScreen background;

        protected override BackgroundScreen CreateBackground() => background;

        public MainMenu()
        {
            background = new BackgroundScreenDefault();

            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem
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
            background.LoadAsync(game);

            buttons.OnSettings = game.ToggleOptions;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            buttons.FadeInFromZero(500);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);

            const float length = 400;

            buttons.State = MenuState.EnteringMode;

            Content.FadeOut(length, EasingTypes.InSine);
            Content.MoveTo(new Vector2(-800, 0), length, EasingTypes.InSine);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            const float length = 300;

            buttons.State = MenuState.TopLevel;

            Content.FadeIn(length, EasingTypes.OutQuint);
            Content.MoveTo(new Vector2(0, 0), length, EasingTypes.OutQuint);
        }

        protected override bool OnExiting(Screen next)
        {
            buttons.State = MenuState.Exit;
            Content.FadeOut(3000);
            return base.OnExiting(next);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && state.Keyboard.ControlPressed && state.Keyboard.ShiftPressed && args.Key == Key.D)
            {
                Push(new Drawings());
                return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
