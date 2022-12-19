// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class MainMenu : OsuScreen, IHandlePresentBeatmap, IKeyBindingHandler<GlobalAction>
    {
        public const float FADE_IN_DURATION = 300;

        public const float FADE_OUT_DURATION = 400;

        public override bool HideOverlaysOnEnter => Buttons == null || Buttons.State == ButtonSystemState.Initial;

        public override bool AllowBackButton => false;

        public override bool AllowExternalScreenChange => true;

        private Screen songSelect;

        private MenuSideFlashes sideFlashes;

        protected ButtonSystem Buttons;

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved(canBeNull: true)]
        private LoginOverlay login { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved(canBeNull: true)]
        private IDialogOverlay dialogOverlay { get; set; }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        protected override bool PlayExitSound => false;

        private Bindable<double> holdDelay;
        private Bindable<bool> loginDisplayed;

        private ExitConfirmOverlay exitConfirmOverlay;

        private ParallaxContainer buttonsContainer;
        private SongTicker songTicker;

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapListingOverlay beatmapListing, SettingsOverlay settings, OsuConfigManager config, SessionStatics statics)
        {
            holdDelay = config.GetBindable<double>(OsuSetting.UIHoldActivationDelay);
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
                        Buttons = new ButtonSystem
                        {
                            OnEdit = delegate
                            {
                                Beatmap.SetDefault();
                                this.Push(new EditorLoader());
                            },
                            OnSolo = loadSoloSongSelect,
                            OnMultiplayer = () => this.Push(new Multiplayer()),
                            OnPlaylists = () => this.Push(new Playlists()),
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
                exitConfirmOverlay?.CreateProxy() ?? Empty()
            });

            Buttons.StateChanged += state =>
            {
                switch (state)
                {
                    case ButtonSystemState.Initial:
                    case ButtonSystemState.Exit:
                        ApplyToBackground(b => b.FadeColour(Color4.White, 500, Easing.OutSine));
                        break;

                    default:
                        ApplyToBackground(b => b.FadeColour(OsuColour.Gray(0.8f), 500, Easing.OutSine));
                        break;
                }
            };

            Buttons.OnSettings = () => settings?.ToggleVisibility();
            Buttons.OnBeatmapListing = () => beatmapListing?.ToggleVisibility();

            preloadSongSelect();
        }

        [Resolved(canBeNull: true)]
        private IPerformFromScreenRunner performer { get; set; }

        public void ReturnToOsuLogo() => Buttons.State = ButtonSystemState.Initial;

        private void confirmAndExit()
        {
            if (exitConfirmed) return;

            exitConfirmed = true;
            performer?.PerformFromScreen(menu => menu.Exit());
        }

        private void preloadSongSelect()
        {
            if (songSelect == null)
                LoadComponentAsync(songSelect = new PlaySongSelect());
        }

        private void loadSoloSongSelect() => this.Push(consumeSongSelect());

        private Screen consumeSongSelect()
        {
            var s = songSelect;
            songSelect = null;
            return s;
        }

        [Resolved]
        private Storage storage { get; set; }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            Buttons.FadeInFromZero(500);

            if (e.Last is IntroScreen && musicController.TrackLoaded)
            {
                var track = musicController.CurrentTrack;

                // presume the track is the current beatmap's track. not sure how correct this assumption is but it has worked until now.
                if (!track.IsRunning)
                {
                    Beatmap.Value.PrepareTrackForPreview(false);
                    track.Restart();
                }
            }

            if (storage is OsuStorage osuStorage && osuStorage.Error != OsuStorageError.None)
                dialogOverlay?.Push(new StorageErrorDialog(osuStorage, osuStorage.Error));
        }

        private bool exitConfirmed;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            Buttons.SetOsuLogo(logo);

            logo.FadeColour(Color4.White, 100, Easing.OutQuint);
            logo.FadeIn(100, Easing.OutQuint);

            if (resuming)
            {
                Buttons.State = ButtonSystemState.TopLevel;

                this.FadeIn(FADE_IN_DURATION, Easing.OutQuint);
                buttonsContainer.MoveTo(new Vector2(0, 0), FADE_IN_DURATION, Easing.OutQuint);

                sideFlashes.Delay(FADE_IN_DURATION).FadeIn(64, Easing.InQuint);
            }
            else if (!api.IsLoggedIn)
            {
                // copy out old action to avoid accidentally capturing logo.Action in closure, causing a self-reference loop.
                var previousAction = logo.Action;

                // we want to hook into logo.Action to display the login overlay, but also preserve the return value of the old action.
                // therefore pass the old action to displayLogin, so that it can return that value.
                // this ensures that the OsuLogo sample does not play when it is not desired.
                logo.Action = () => displayLogin(previousAction);
            }

            bool displayLogin(Func<bool> originalAction)
            {
                if (!loginDisplayed.Value)
                {
                    Scheduler.AddDelayed(() => login?.Show(), 500);
                    loginDisplayed.Value = true;
                }

                return originalAction.Invoke();
            }
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            var seq = logo.FadeOut(300, Easing.InSine)
                          .ScaleTo(0.2f, 300, Easing.InSine);

            seq.OnComplete(_ => Buttons.SetOsuLogo(null));
            seq.OnAbort(_ => Buttons.SetOsuLogo(null));
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            Buttons.State = ButtonSystemState.EnteringMode;

            this.FadeOut(FADE_OUT_DURATION, Easing.InSine);
            buttonsContainer.MoveTo(new Vector2(-800, 0), FADE_OUT_DURATION, Easing.InSine);

            sideFlashes.FadeOut(64, Easing.OutQuint);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            ApplyToBackground(b => (b as BackgroundScreenDefault)?.Next());

            // we may have consumed our preloaded instance, so let's make another.
            preloadSongSelect();

            musicController.EnsurePlayingSomething();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!exitConfirmed && dialogOverlay != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmExitDialog exitDialog)
                    exitDialog.PerformOkAction();
                else
                    dialogOverlay.Push(new ConfirmExitDialog(confirmAndExit, () => exitConfirmOverlay.Abort()));

                return true;
            }

            Buttons.State = ButtonSystemState.Exit;
            OverlayActivationMode.Value = OverlayActivation.Disabled;

            songTicker.Hide();

            this.FadeOut(3000);
            return base.OnExiting(e);
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            Logger.Log($"{nameof(MainMenu)} completing {nameof(PresentBeatmap)} with beatmap {beatmap} ruleset {ruleset}");

            Beatmap.Value = beatmap;
            Ruleset.Value = ruleset;

            Schedule(loadSoloSongSelect);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    // In the case of a host being able to exit, the back action is handled by ExitConfirmOverlay.
                    Debug.Assert(!host.CanExit);

                    return host.SuspendToBackground();
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
