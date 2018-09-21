// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Charts;
using osu.Game.Screens.Direct;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Select;
using osu.Game.Screens.Tournament;

namespace osu.Game.Screens.Menu
{
    public class MainMenu : OsuScreen
    {
        private readonly ButtonSystem buttons;

        protected override bool HideOverlaysOnEnter => buttons.State == ButtonSystemState.Initial;

        protected override bool AllowBackButton => buttons.State != ButtonSystemState.Initial;

        private readonly BackgroundScreenDefault background;
        private Screen songSelect;

        private readonly MenuSideFlashes sideFlashes;

        protected override BackgroundScreen CreateBackground() => background;

        public MainMenu()
        {
            background = new BackgroundScreenDefault();

            Children = new Drawable[]
            {
                new ExitConfirmOverlay
                {
                    Action = Exit,
                },
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem
                        {
                            OnChart = delegate { Push(new ChartListing()); },
                            OnDirect = delegate { Push(new OnlineListing()); },
                            OnEdit = delegate { Push(new Editor()); },
                            OnSolo = onSolo,
                            OnMulti = delegate { Push(new Multiplayer()); },
                            OnExit = Exit,
                        }
                    }
                },
                sideFlashes = new MenuSideFlashes(),
            };

            buttons.StateChanged += state =>
            {
                switch (state)
                {
                    case ButtonSystemState.Initial:
                    case ButtonSystemState.Exit:
                        background.FadeColour(Color4.White, 500, Easing.OutSine);
                        break;
                    default:
                        background.FadeColour(OsuColour.Gray(0.8f), 500, Easing.OutSine);
                        break;
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game = null)
        {
            LoadComponentAsync(background);

            if (game != null)
            {
                buttons.OnSettings = game.ToggleSettings;
                buttons.OnDirect = game.ToggleDirect;
            }

            preloadSongSelect();
        }

        private void preloadSongSelect()
        {
            if (songSelect == null)
                LoadComponentAsync(songSelect = new PlaySongSelect());
        }

        public void LoadToSolo() => Schedule(onSolo);

        private void onSolo() => Push(consumeSongSelect());

        private Screen consumeSongSelect()
        {
            var s = songSelect;
            songSelect = null;
            return s;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            buttons.FadeInFromZero(500);

            var track = Beatmap.Value.Track;
            var metadata = Beatmap.Value.Metadata;

            if (last is Intro && track != null)
            {
                if (!track.IsRunning)
                {
                    track.Seek(metadata.PreviewTime != -1 ? metadata.PreviewTime : 0.4f * track.Length);
                    track.Start();
                }
            }

            Beatmap.ValueChanged += beatmap_ValueChanged;
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            buttons.SetOsuLogo(logo);

            logo.FadeColour(Color4.White, 100, Easing.OutQuint);
            logo.FadeIn(100, Easing.OutQuint);

            if (resuming)
            {
                buttons.State = ButtonSystemState.TopLevel;

                const float length = 300;

                Content.FadeIn(length, Easing.OutQuint);
                Content.MoveTo(new Vector2(0, 0), length, Easing.OutQuint);

                sideFlashes.Delay(length).FadeIn(64, Easing.InQuint);
            }
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            logo.FadeOut(300, Easing.InSine)
                .ScaleTo(0.2f, 300, Easing.InSine)
                .OnComplete(l => buttons.SetOsuLogo(null));
        }

        private void beatmap_ValueChanged(WorkingBeatmap newValue)
        {
            if (!IsCurrentScreen)
                return;

            background.Next();
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);

            const float length = 400;

            buttons.State = ButtonSystemState.EnteringMode;

            Content.FadeOut(length, Easing.InSine);
            Content.MoveTo(new Vector2(-800, 0), length, Easing.InSine);

            sideFlashes.FadeOut(64, Easing.OutQuint);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            background.Next();

            //we may have consumed our preloaded instance, so let's make another.
            preloadSongSelect();
        }

        protected override bool OnExiting(Screen next)
        {
            buttons.State = ButtonSystemState.Exit;
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
