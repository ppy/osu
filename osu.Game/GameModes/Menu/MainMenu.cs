//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.GameModes.Backgrounds;
using osu.Game.GameModes.Charts;
using osu.Game.GameModes.Direct;
using osu.Game.GameModes.Edit;
using osu.Game.GameModes.Multiplayer;
using osu.Game.GameModes.Play;
using osu.Game.Graphics.Containers;
using OpenTK;
using osu.Framework;
using osu.Game.Overlays;

namespace osu.Game.GameModes.Menu
{
    public class MainMenu : OsuGameMode
    {
        private ButtonSystem buttons;
        public override string Name => @"Main Menu";

        protected override BackgroundMode CreateBackground() => new BackgroundModeDefault();

        public override void Load(BaseGame game)
        {
            base.Load(game);

            OsuGame osu = (OsuGame)game;

            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem()
                        {
                            Alpha = 0,
                            OnChart = delegate { Push(new ChartListing()); },
                            OnDirect = delegate { Push(new OnlineListing()); },
                            OnEdit = delegate { Push(new EditSongSelect()); },
                            OnSolo = delegate { Push(new PlaySongSelect()); },
                            OnMulti = delegate { Push(new Lobby()); },
                            OnTest  = delegate { Push(new TestBrowser()); },
                            OnExit = delegate { Scheduler.AddDelayed(Exit, ButtonSystem.EXIT_DELAY); },
                            OnSettings = osu.Options.ToggleVisibility,
                        }
                    }
                }
            };

            buttons.FadeIn(500);
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
    }
}
