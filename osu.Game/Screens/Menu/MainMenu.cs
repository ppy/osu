// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Charts;
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

        public override bool AllowRateAdjustments => false;

        private Screen songSelect;

        private MenuSideFlashes sideFlashes;

        private ButtonSystem buttons;

        [Resolved]
        private GameHost host { get; set; }

        [Resolved(canBeNull: true)]
        private MusicController music { get; set; }

        [Resolved(canBeNull: true)]
        private LoginOverlay login { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        private BackgroundScreenDefault background;

        protected override BackgroundScreen CreateBackground() => background;

        private Bindable<float> holdDelay;
        private Bindable<bool> loginDisplayed;

        private ExitConfirmOverlay exitConfirmOverlay;

        private ParallaxContainer buttonsContainer;
        private SongTicker songTicker;

        [BackgroundDependencyLoader(true)]
        private void load(DirectOverlay direct, SettingsOverlay settings, OsuConfigManager config, SessionStatics statics)
        {
            holdDelay = config.GetBindable<float>(OsuSetting.UIHoldActivationDelay);
            loginDisplayed = statics.GetBindable<bool>(Static.LoginOverlayDisplayed);

            if (host.CanExit)
            {
                AddInternal(exitConfirmOverlay = new ExitConfirmOverlay
                {
                    Action = () =>
                    {
                        if (holdDelay.Value > 0)
                            confirmAndExit();
                        else
                            this.Exit();
                    }
                });
            }

            AddRangeInternal(new[]
            {
                buttonsContainer = new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem
                        {
                            OnChart = delegate { this.Push(new ChartListing()); },
                            OnEdit = delegate { this.Push(new Editor()); },
                            OnSolo = onSolo,
                            OnMulti = delegate { this.Push(new Multiplayer()); },
                            OnExit = confirmAndExit,
                        }
                    }
                },
                sideFlashes = new MenuSideFlashes(),
                songTicker = new SongTicker
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 15, Top = 5 }
                },
                exitConfirmOverlay?.CreateProxy() ?? Drawable.Empty()
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

        [Resolved]
        private OsuGame game { get; set; }

        private void confirmAndExit()
        {
            if (exitConfirmed) return;

            exitConfirmed = true;
            game.PerformFromScreen(menu => menu.Exit());
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

            if (last is IntroScreen && track != null)
            {
                if (!track.IsRunning)
                {
                    track.Seek(metadata.PreviewTime != -1 ? metadata.PreviewTime : 0.4f * track.Length);
                    track.Start();
                }
            }
        }

        private bool exitConfirmed;

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
                buttonsContainer.MoveTo(new Vector2(0, 0), FADE_IN_DURATION, Easing.OutQuint);

                sideFlashes.Delay(FADE_IN_DURATION).FadeIn(64, Easing.InQuint);
            }
            else if (!api.IsLoggedIn)
            {
                logo.Action += displayLogin;
            }

            bool displayLogin()
            {
                if (!loginDisplayed.Value)
                {
                    Scheduler.AddDelayed(() => login?.Show(), 500);
                    loginDisplayed.Value = true;
                }

                return true;
            }
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            var seq = logo.FadeOut(300, Easing.InSine)
                          .ScaleTo(0.2f, 300, Easing.InSine);

            seq.OnComplete(_ => buttons.SetOsuLogo(null));
            seq.OnAbort(_ => buttons.SetOsuLogo(null));
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            buttons.State = ButtonSystemState.EnteringMode;

            this.FadeOut(FADE_OUT_DURATION, Easing.InSine);
            buttonsContainer.MoveTo(new Vector2(-800, 0), FADE_OUT_DURATION, Easing.InSine);

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
            if (!exitConfirmed && dialogOverlay != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmExitDialog exitDialog)
                {
                    exitConfirmed = true;
                    exitDialog.Buttons.First().Click();
                }
                else
                {
                    dialogOverlay.Push(new ConfirmExitDialog(confirmAndExit, () => exitConfirmOverlay.Abort()));
                    return true;
                }
            }

            buttons.State = ButtonSystemState.Exit;

            songTicker.Hide();

            this.FadeOut(3000);
            return base.OnExiting(next);
        }

        private class ConfirmExitDialog : PopupDialog
        {
            public ConfirmExitDialog(Action confirm, Action cancel)
            {
                HeaderText = "Are you sure you want to exit?";
                BodyText = "Last chance to back out.";

                Icon = FontAwesome.Solid.ExclamationTriangle;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"Good bye",
                        Action = confirm
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Just a little more",
                        Action = cancel
                    },
                };
            }
        }
    }
}
