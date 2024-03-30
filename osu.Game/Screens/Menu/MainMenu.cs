// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.SkinEditor;
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

        public override bool? AllowGlobalTrackControl => true;

        private Screen songSelect;

        private MenuSideFlashes sideFlashes;

        protected ButtonSystem Buttons;

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private INotificationOverlay notifications { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Storage storage { get; set; }

        [Resolved(canBeNull: true)]
        private LoginOverlay login { get; set; }

        [Resolved(canBeNull: true)]
        private IDialogOverlay dialogOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private VersionManager versionManager { get; set; }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        protected override bool PlayExitSound => false;

        private Bindable<double> holdDelay;
        private Bindable<bool> loginDisplayed;

        private HoldToExitGameOverlay holdToExitGameOverlay;

        private bool exitConfirmedViaDialog;
        private bool exitConfirmedViaHoldOrClick;

        private ParallaxContainer buttonsContainer;
        private SongTicker songTicker;
        private Container logoTarget;
        private OnlineMenuBanner onlineMenuBanner;
        private MenuTip menuTip;
        private FillFlowContainer bottomElementsFlow;
        private SupporterDisplay supporterDisplay;

        private Sample reappearSampleSwoosh;

        [Resolved(canBeNull: true)]
        private SkinEditorOverlay skinEditor { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapListingOverlay beatmapListing, SettingsOverlay settings, OsuConfigManager config, SessionStatics statics, AudioManager audio)
        {
            holdDelay = config.GetBindable<double>(OsuSetting.UIHoldActivationDelay);
            loginDisplayed = statics.GetBindable<bool>(Static.LoginOverlayDisplayed);

            if (host.CanExit)
            {
                AddInternal(holdToExitGameOverlay = new HoldToExitGameOverlay
                {
                    Action = () =>
                    {
                        exitConfirmedViaHoldOrClick = holdDelay.Value > 0;
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
                            OnEditBeatmap = () =>
                            {
                                Beatmap.SetDefault();
                                this.Push(new EditorLoader());
                            },
                            OnEditSkin = () =>
                            {
                                skinEditor?.Show();
                            },
                            OnSolo = loadSoloSongSelect,
                            OnMultiplayer = () => this.Push(new Multiplayer()),
                            OnPlaylists = () => this.Push(new Playlists()),
                            OnExit = () =>
                            {
                                exitConfirmedViaHoldOrClick = true;
                                this.Exit();
                            }
                        }
                    }
                },
                logoTarget = new Container { RelativeSizeAxes = Axes.Both, },
                sideFlashes = new MenuSideFlashes(),
                songTicker = new SongTicker
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 15, Top = 5 }
                },
                new KiaiMenuFountains(),
                bottomElementsFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        menuTip = new MenuTip
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        onlineMenuBanner = new OnlineMenuBanner
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        }
                    }
                },
                supporterDisplay = new SupporterDisplay
                {
                    Margin = new MarginPadding(5),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                holdToExitGameOverlay?.CreateProxy() ?? Empty()
            });

            Buttons.StateChanged += state =>
            {
                switch (state)
                {
                    case ButtonSystemState.Initial:
                    case ButtonSystemState.Exit:
                        ApplyToBackground(b => b.FadeColour(Color4.White, 500, Easing.OutSine));
                        onlineMenuBanner.State.Value = Visibility.Hidden;
                        break;

                    default:
                        ApplyToBackground(b => b.FadeColour(OsuColour.Gray(0.8f), 500, Easing.OutSine));
                        onlineMenuBanner.State.Value = Visibility.Visible;
                        break;
                }
            };

            Buttons.OnSettings = () => settings?.ToggleVisibility();
            Buttons.OnBeatmapListing = () => beatmapListing?.ToggleVisibility();

            reappearSampleSwoosh = audio.Samples.Get(@"Menu/reappear-swoosh");

            preloadSongSelect();
        }

        public void ReturnToOsuLogo() => Buttons.State = ButtonSystemState.Initial;

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

        [CanBeNull]
        private Drawable proxiedLogo;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            Buttons.SetOsuLogo(logo);

            logo.FadeColour(Color4.White, 100, Easing.OutQuint);
            logo.FadeIn(100, Easing.OutQuint);

            proxiedLogo = logo.ProxyToContainer(logoTarget);

            if (resuming)
            {
                Buttons.State = ButtonSystemState.TopLevel;

                this.FadeIn(FADE_IN_DURATION, Easing.OutQuint);
                buttonsContainer.MoveTo(new Vector2(0, 0), FADE_IN_DURATION, Easing.OutQuint);

                sideFlashes.Delay(FADE_IN_DURATION).FadeIn(64, Easing.InQuint);
            }
            else if (!api.IsLoggedIn || api.State.Value == APIState.RequiresSecondFactorAuth)
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

        protected override void Update()
        {
            base.Update();

            bottomElementsFlow.Margin = new MarginPadding
            {
                Bottom = (versionManager?.DrawHeight + 5) ?? 0
            };
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            var seq = logo.FadeOut(300, Easing.InSine)
                          .ScaleTo(0.2f, 300, Easing.InSine);

            if (proxiedLogo != null)
            {
                logo.ReturnProxy();
                proxiedLogo = null;
            }

            seq.OnComplete(_ => Buttons.SetOsuLogo(null));
            seq.OnAbort(_ => Buttons.SetOsuLogo(null));
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);

            if (proxiedLogo != null)
            {
                logo.ReturnProxy();
                proxiedLogo = null;
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            Buttons.State = ButtonSystemState.EnteringMode;

            this.FadeOut(FADE_OUT_DURATION, Easing.InSine);
            buttonsContainer.MoveTo(new Vector2(-800, 0), FADE_OUT_DURATION, Easing.InSine);

            sideFlashes.FadeOut(64, Easing.OutQuint);

            bottomElementsFlow
                .ScaleTo(0.9f, 1000, Easing.OutQuint)
                .FadeOut(500, Easing.OutQuint);

            supporterDisplay
                .FadeOut(500, Easing.OutQuint);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            // Ensures any playing `ButtonSystem` samples are stopped when returning to MainMenu (as to not overlap with the 'back' sample)
            Buttons.StopSamplePlayback();
            reappearSampleSwoosh?.Play();

            ApplyToBackground(b => (b as BackgroundScreenDefault)?.Next());

            // we may have consumed our preloaded instance, so let's make another.
            preloadSongSelect();

            musicController.EnsurePlayingSomething();

            // Cycle tip on resuming
            menuTip.ShowNextTip();

            bottomElementsFlow
                .ScaleTo(1, 1000, Easing.OutQuint)
                .FadeIn(1000, Easing.OutQuint);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            bool requiresConfirmation =
                // we need to have a dialog overlay to confirm in the first place.
                dialogOverlay != null
                // if the dialog has already displayed and been accepted by the user, we are good.
                && !exitConfirmedViaDialog
                // Only require confirmation if there is either an ongoing operation or the user exited via a non-hold escape press.
                && (notifications.HasOngoingOperations || !exitConfirmedViaHoldOrClick);

            if (requiresConfirmation)
            {
                if (dialogOverlay.CurrentDialog is ConfirmExitDialog exitDialog)
                {
                    if (exitDialog.Buttons.OfType<PopupDialogOkButton>().FirstOrDefault() != null)
                        exitDialog.PerformOkAction();
                    else
                        exitDialog.Flash();
                }
                else
                {
                    dialogOverlay.Push(new ConfirmExitDialog(() =>
                    {
                        exitConfirmedViaDialog = true;
                        this.Exit();
                    }, () =>
                    {
                        holdToExitGameOverlay.Abort();
                    }));
                }

                return true;
            }

            Buttons.State = ButtonSystemState.Exit;
            OverlayActivationMode.Value = OverlayActivation.Disabled;

            songTicker.Hide();

            this.FadeOut(3000);

            bottomElementsFlow
                .FadeOut(500, Easing.OutQuint);

            supporterDisplay
                .FadeOut(500, Easing.OutQuint);

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
