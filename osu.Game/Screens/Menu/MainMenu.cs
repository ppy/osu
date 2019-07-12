// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Charts;
using osu.Game.Screens.Direct;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Menu
{
    public class MainMenu : OsuScreen
    {
        public const float FADE_IN_DURATION = 300;

        public const float FADE_OUT_DURATION = 400;

        public override bool HideOverlaysOnEnter => buttons == null || buttons.State == ButtonSystemState.Initial;

        public override bool AllowBackButton => false;

        public override bool AllowExternalScreenChange => true;

        private Screen songSelect;

        private MenuSideFlashes sideFlashes;

        private ButtonSystem buttons;

        [Resolved]
        private GameHost host { get; set; }

        [Resolved(canBeNull: true)]
        private MusicController music { get; set; }

        private BackgroundScreenDefault background;

        protected override BackgroundScreen CreateBackground() => background;

        [BackgroundDependencyLoader(true)]
        private void load(DirectOverlay direct, SettingsOverlay settings)
        {
            if (host.CanExit)
                AddInternal(new ExitConfirmOverlay { Action = this.Exit });

            AddRangeInternal(new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem
                        {
                            OnChart = delegate { this.Push(new ChartListing()); },
                            OnDirect = delegate { this.Push(new OnlineListing()); },
                            OnEdit = delegate { this.Push(new Editor()); },
                            OnSolo = onSolo,
                            OnMulti = delegate { this.Push(new Multiplayer()); },
                            OnExit = this.Exit,
                        }
                    }
                },
                sideFlashes = new MenuSideFlashes(),
            });

            buttons.StateChanged += state =>
            {
                switch (state)
                {
                    case ButtonSystemState.Initial:
                    case ButtonSystemState.Exit:
                        Background.FadeColour(Color4.White, 500, Easing.OutSine);
                        break;

                    default:
                        Background.FadeColour(OsuColour.Gray(0.8f), 500, Easing.OutSine);
                        break;
                }
            };

            buttons.OnSettings = () => settings?.ToggleVisibility();
            buttons.OnDirect = () => direct?.ToggleVisibility();

            LoadComponentAsync(background = new BackgroundScreenDefault());
            preloadSongSelect();
        }

        private void preloadSongSelect()
        {
            if (songSelect == null)
                LoadComponentAsync(songSelect = new PlaySongSelect());
        }

        public void LoadToSolo() => Schedule(onSolo);

        private void onSolo() => this.Push(consumeSongSelect());

        private Screen consumeSongSelect()
        {
            var s = songSelect;
            songSelect = null;
            return s;
        }

        public override void OnEntering(IScreen last)
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

                this.FadeIn(FADE_IN_DURATION, Easing.OutQuint);
                this.MoveTo(new Vector2(0, 0), FADE_IN_DURATION, Easing.OutQuint);

                sideFlashes.Delay(FADE_IN_DURATION).FadeIn(64, Easing.InQuint);
            }
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            var seq = logo.FadeOut(300, Easing.InSine)
                          .ScaleTo(0.2f, 300, Easing.InSine);

            seq.OnComplete(_ => buttons.SetOsuLogo(null));
            seq.OnAbort(_ => buttons.SetOsuLogo(null));
        }

        private void beatmap_ValueChanged(ValueChangedEvent<WorkingBeatmap> e)
        {
            if (!this.IsCurrentScreen())
                return;

            ((BackgroundScreenDefault)Background).Next();
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            buttons.State = ButtonSystemState.EnteringMode;

            this.FadeOut(FADE_OUT_DURATION, Easing.InSine);
            this.MoveTo(new Vector2(-800, 0), FADE_OUT_DURATION, Easing.InSine);

            sideFlashes.FadeOut(64, Easing.OutQuint);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            (Background as BackgroundScreenDefault)?.Next();

            //we may have consumed our preloaded instance, so let's make another.
            preloadSongSelect();

            if (Beatmap.Value.Track != null && music?.IsUserPaused != true)
                Beatmap.Value.Track.Start();
        }

        public override bool OnExiting(IScreen next)
        {
            buttons.State = ButtonSystemState.Exit;
            this.FadeOut(3000);
            return base.OnExiting(next);
        }
    }
}
