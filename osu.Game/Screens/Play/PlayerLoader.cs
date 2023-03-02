﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Audio.Effects;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public partial class PlayerLoader : ScreenWithBeatmapBackground
    {
        protected const float BACKGROUND_BLUR = 15;

        protected const double CONTENT_OUT_DURATION = 300;

        protected virtual double PlayerPushDelay => 1800;

        public override bool HideOverlaysOnEnter => hideOverlays;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // Here because IsHovered will not update unless we do so.
        public override bool HandlePositionalInput => true;

        // We show the previous screen status
        protected override UserActivity? InitialActivity => null;

        protected BeatmapMetadataDisplay MetadataInfo { get; private set; } = null!;

        /// <summary>
        /// A fill flow containing the player settings groups, exposed for the ability to hide it from inheritors of the player loader.
        /// </summary>
        protected FillFlowContainer<PlayerSettingsGroup> PlayerSettings { get; private set; } = null!;

        protected VisualSettings VisualSettings { get; private set; } = null!;

        protected AudioSettings AudioSettings { get; private set; } = null!;

        protected Task? LoadTask { get; private set; }

        protected Task? DisposalTask { get; private set; }

        private OsuScrollContainer settingsScroll = null!;

        private bool backgroundBrightnessReduction;

        private readonly BindableDouble volumeAdjustment = new BindableDouble(1);

        private AudioFilter lowPassFilter = null!;
        private AudioFilter highPassFilter = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        protected bool BackgroundBrightnessReduction
        {
            set
            {
                if (value == backgroundBrightnessReduction)
                    return;

                backgroundBrightnessReduction = value;

                ApplyToBackground(b => b.FadeColour(OsuColour.Gray(backgroundBrightnessReduction ? 0.8f : 1), 200));
            }
        }

        private bool readyForPush =>
            !playerConsumed
            // don't push unless the player is completely loaded
            && CurrentPlayer?.LoadState == LoadState.Ready
            // don't push unless the player is ready to start gameplay
            && ReadyForGameplay;

        protected virtual bool ReadyForGameplay =>
            // not ready if the user is hovering one of the panes, unless they are idle.
            (IsHovered || idleTracker.IsIdle.Value)
            // not ready if the user is dragging a slider or otherwise.
            && inputManager.DraggedDrawable == null
            // not ready if a focused overlay is visible, like settings.
            && inputManager.FocusedDrawable == null;

        private readonly Func<Player> createPlayer;

        /// <summary>
        /// The <see cref="Player"/> instance being loaded by this screen.
        /// </summary>
        public Player? CurrentPlayer { get; private set; }

        /// <summary>
        /// Whether the current player instance has been consumed via <see cref="consumePlayer"/>.
        /// </summary>
        private bool playerConsumed;

        private LogoTrackingContainer content = null!;

        private bool hideOverlays;

        private InputManager inputManager = null!;

        private IdleTracker idleTracker = null!;

        private ScheduledDelegate? scheduledPushPlayer;

        private EpilepsyWarning? epilepsyWarning;

        private bool quickRestart;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [Resolved]
        private VolumeOverlay? volumeOverlay { get; set; }

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        [Resolved]
        private BatteryInfo? batteryInfo { get; set; }

        public PlayerLoader(Func<Player> createPlayer)
        {
            this.createPlayer = createPlayer;
        }

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics, AudioManager audio)
        {
            muteWarningShownOnce = sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce);
            batteryWarningShownOnce = sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce);

            const float padding = 25;

            InternalChildren = new Drawable[]
            {
                (content = new LogoTrackingContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                }).WithChildren(new Drawable[]
                {
                    MetadataInfo = new BeatmapMetadataDisplay(Beatmap.Value, Mods, content.LogoFacade)
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                }),
                settingsScroll = new OsuScrollContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = SettingsToolboxGroup.CONTAINER_WIDTH + padding * 2,
                    Padding = new MarginPadding { Vertical = padding },
                    Masking = false,
                    Child = PlayerSettings = new FillFlowContainer<PlayerSettingsGroup>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Padding = new MarginPadding { Horizontal = padding },
                        Children = new PlayerSettingsGroup[]
                        {
                            VisualSettings = new VisualSettings(),
                            AudioSettings = new AudioSettings(),
                            new InputSettings()
                        }
                    },
                },
                idleTracker = new IdleTracker(750),
                lowPassFilter = new AudioFilter(audio.TrackMixer),
                highPassFilter = new AudioFilter(audio.TrackMixer, BQFType.HighPass)
            };

            if (Beatmap.Value.BeatmapInfo.EpilepsyWarning)
            {
                AddInternal(epilepsyWarning = new EpilepsyWarning
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        #region Screen handling

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            ApplyToBackground(b =>
            {
                if (epilepsyWarning != null)
                    epilepsyWarning.DimmableBackground = b;
            });

            Beatmap.Value.Track.AddAdjustment(AdjustableProperty.Volume, volumeAdjustment);

            // Start off-screen.
            settingsScroll.MoveToX(settingsScroll.DrawWidth);

            content.ScaleTo(0.7f);

            contentIn();

            MetadataInfo.Delay(750).FadeIn(500);

            // after an initial delay, start the debounced load check.
            // this will continue to execute even after resuming back on restart.
            Scheduler.Add(new ScheduledDelegate(pushWhenLoaded, Clock.CurrentTime + PlayerPushDelay, 0));

            showMuteWarningIfNeeded();
            showBatteryWarningIfNeeded();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            Debug.Assert(CurrentPlayer != null);

            var lastScore = CurrentPlayer.Score;

            AudioSettings.ReferenceScore.Value = lastScore?.ScoreInfo;

            // prepare for a retry.
            CurrentPlayer = null;
            playerConsumed = false;
            cancelLoad();

            contentIn();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            BackgroundBrightnessReduction = false;

            // we're moving to player, so a period of silence is upcoming.
            // stop the track before removing adjustment to avoid a volume spike.
            Beatmap.Value.Track.Stop();
            Beatmap.Value.Track.RemoveAdjustment(AdjustableProperty.Volume, volumeAdjustment);
            lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF);
            highPassFilter.CutoffTo(0);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            cancelLoad();
            ContentOut();

            // If the load sequence was interrupted, the epilepsy warning may already be displayed (or in the process of being displayed).
            epilepsyWarning?.Hide();

            // Ensure the screen doesn't expire until all the outwards fade operations have completed.
            this.Delay(CONTENT_OUT_DURATION).FadeOut();

            ApplyToBackground(b => b.IgnoreUserSettings.Value = true);

            BackgroundBrightnessReduction = false;
            Beatmap.Value.Track.RemoveAdjustment(AdjustableProperty.Volume, volumeAdjustment);

            return base.OnExiting(e);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            const double duration = 300;

            if (!resuming) logo.MoveTo(new Vector2(0.5f), duration, Easing.OutQuint);

            logo.ScaleTo(new Vector2(0.15f), duration, Easing.OutQuint);
            logo.FadeIn(350);

            Scheduler.AddDelayed(() =>
            {
                if (this.IsCurrentScreen())
                    content.StartTracking(logo, resuming ? 0 : 500, Easing.InOutExpo);
            }, resuming ? 0 : 500);
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            content.StopTracking();
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            content.StopTracking();

            logo
                .FadeOut(CONTENT_OUT_DURATION / 2, Easing.OutQuint)
                .ScaleTo(logo.Scale * 0.8f, CONTENT_OUT_DURATION * 2, Easing.OutQuint);
        }

        #endregion

        protected override void Update()
        {
            base.Update();

            if (!this.IsCurrentScreen())
                return;

            // We need to perform this check here rather than in OnHover as any number of children of VisualSettings
            // may also be handling the hover events.
            if (inputManager.HoveredDrawables.Contains(VisualSettings))
            {
                // Preview user-defined background dim and blur when hovered on the visual settings panel.
                ApplyToBackground(b =>
                {
                    b.IgnoreUserSettings.Value = false;
                    b.BlurAmount.Value = 0;
                });

                BackgroundBrightnessReduction = false;
            }
            else
            {
                ApplyToBackground(b =>
                {
                    // Returns background dim and blur to the values specified by PlayerLoader.
                    b.IgnoreUserSettings.Value = true;
                    b.BlurAmount.Value = BACKGROUND_BLUR;
                });

                BackgroundBrightnessReduction = true;
            }
        }

        private Player consumePlayer()
        {
            Debug.Assert(!playerConsumed);
            Debug.Assert(CurrentPlayer != null);

            playerConsumed = true;
            return CurrentPlayer;
        }

        private void prepareNewPlayer()
        {
            if (!this.IsCurrentScreen())
                return;

            CurrentPlayer = createPlayer();
            CurrentPlayer.Configuration.AutomaticallySkipIntro |= quickRestart;
            CurrentPlayer.RestartCount = restartCount++;
            CurrentPlayer.RestartRequested = restartRequested;

            LoadTask = LoadComponentAsync(CurrentPlayer, _ =>
            {
                MetadataInfo.Loading = false;
                OnPlayerLoaded();
            });
        }

        protected virtual void OnPlayerLoaded()
        {
        }

        private void restartRequested(bool quickRestartRequested)
        {
            quickRestart = quickRestartRequested;
            hideOverlays = true;
            ValidForResume = true;
        }

        private void contentIn()
        {
            MetadataInfo.Loading = true;

            content.FadeInFromZero(400);
            content.ScaleTo(1, 650, Easing.OutQuint).Then().Schedule(prepareNewPlayer);

            settingsScroll.FadeInFromZero(500, Easing.Out)
                          .MoveToX(0, 500, Easing.OutQuint);

            lowPassFilter.CutoffTo(1000, 650, Easing.OutQuint);
            highPassFilter.CutoffTo(300).Then().CutoffTo(0, 1250); // 1250 is to line up with the appearance of MetadataInfo (750 delay + 500 fade-in)

            ApplyToBackground(b => b.FadeColour(Color4.White, 800, Easing.OutQuint));
        }

        protected virtual void ContentOut()
        {
            // Ensure the logo is no longer tracking before we scale the content
            content.StopTracking();

            content.ScaleTo(0.7f, CONTENT_OUT_DURATION * 2, Easing.OutQuint);
            content.FadeOut(CONTENT_OUT_DURATION, Easing.OutQuint);

            settingsScroll.FadeOut(CONTENT_OUT_DURATION, Easing.OutQuint)
                          .MoveToX(settingsScroll.DrawWidth, CONTENT_OUT_DURATION * 2, Easing.OutQuint);

            lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, CONTENT_OUT_DURATION);
            highPassFilter.CutoffTo(0, CONTENT_OUT_DURATION);
        }

        private void pushWhenLoaded()
        {
            if (!this.IsCurrentScreen()) return;

            if (!readyForPush)
            {
                // as the pushDebounce below has a delay, we need to keep checking and cancel a future debounce
                // if we become unready for push during the delay.
                cancelLoad();
                return;
            }

            // if a push has already been scheduled, no further action is required.
            // this value is reset via cancelLoad() to allow a second usage of the same PlayerLoader screen.
            if (scheduledPushPlayer != null)
                return;

            scheduledPushPlayer = Scheduler.AddDelayed(() =>
            {
                // ensure that once we have reached this "point of no return", readyForPush will be false for all future checks (until a new player instance is prepared).
                var consumedPlayer = consumePlayer();

                ContentOut();

                TransformSequence<PlayerLoader> pushSequence = this.Delay(0);

                // only show if the warning was created (i.e. the beatmap needs it)
                // and this is not a restart of the map (the warning expires after first load).
                if (epilepsyWarning?.IsAlive == true)
                {
                    const double epilepsy_display_length = 3000;

                    pushSequence
                        .Delay(CONTENT_OUT_DURATION)
                        .Schedule(() => epilepsyWarning.State.Value = Visibility.Visible)
                        .TransformBindableTo(volumeAdjustment, 0.25, EpilepsyWarning.FADE_DURATION, Easing.OutQuint)
                        .Delay(epilepsy_display_length)
                        .Schedule(() =>
                        {
                            epilepsyWarning.Hide();
                            epilepsyWarning.Expire();
                        })
                        .Delay(EpilepsyWarning.FADE_DURATION);
                }
                else
                {
                    // This goes hand-in-hand with the restoration of low pass filter in contentOut().
                    this.TransformBindableTo(volumeAdjustment, 0, CONTENT_OUT_DURATION, Easing.OutCubic);
                }

                pushSequence.Schedule(() =>
                {
                    if (!this.IsCurrentScreen()) return;

                    LoadTask = null;

                    // By default, we want to load the player and never be returned to.
                    // Note that this may change if the player we load requested a re-run.
                    ValidForResume = false;

                    if (consumedPlayer.LoadedBeatmapSuccessfully)
                        this.Push(consumedPlayer);
                    else
                        this.Exit();
                });
            }, 500);
        }

        private void cancelLoad()
        {
            scheduledPushPlayer?.Cancel();
            scheduledPushPlayer = null;
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
            {
                // if the player never got pushed, we should explicitly dispose it.
                DisposalTask = LoadTask?.ContinueWith(_ => CurrentPlayer?.Dispose());
            }
        }

        #endregion

        #region Mute warning

        private Bindable<bool> muteWarningShownOnce = null!;

        private int restartCount;

        private const double volume_requirement = 0.01;

        private void showMuteWarningIfNeeded()
        {
            if (!muteWarningShownOnce.Value)
            {
                // Checks if the notification has not been shown yet and also if master volume is muted, track/music volume is muted or if the whole game is muted.
                if (volumeOverlay?.IsMuted.Value == true || audioManager.Volume.Value <= volume_requirement || audioManager.VolumeTrack.Value <= volume_requirement)
                {
                    notificationOverlay?.Post(new MutedNotification());
                    muteWarningShownOnce.Value = true;
                }
            }
        }

        private partial class MutedNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public MutedNotification()
            {
                Text = NotificationsStrings.GameVolumeTooLow;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audioManager, INotificationOverlay notificationOverlay, VolumeOverlay volumeOverlay)
            {
                Icon = FontAwesome.Solid.VolumeMute;
                IconContent.Colour = colours.RedDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();

                    volumeOverlay.IsMuted.Value = false;

                    // Check values before resetting, as the user may have only had mute enabled, in which case we might not need to adjust volumes.
                    // Note that we only restore halfway to ensure the user isn't suddenly overloaded by unexpectedly high volume.
                    if (audioManager.Volume.Value <= volume_requirement)
                        audioManager.Volume.Value = 0.5f;
                    if (audioManager.VolumeTrack.Value <= volume_requirement)
                        audioManager.VolumeTrack.Value = 0.5f;

                    return true;
                };
            }
        }

        #endregion

        #region Low battery warning

        private const double low_battery_threshold = 0.25;

        private Bindable<bool> batteryWarningShownOnce = null!;

        private void showBatteryWarningIfNeeded()
        {
            if (batteryInfo == null) return;

            if (!batteryWarningShownOnce.Value)
            {
                if (batteryInfo.OnBattery && batteryInfo.ChargeLevel <= low_battery_threshold)
                {
                    notificationOverlay?.Post(new BatteryWarningNotification());
                    batteryWarningShownOnce.Value = true;
                }
            }
        }

        private partial class BatteryWarningNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public BatteryWarningNotification()
            {
                Text = NotificationsStrings.BatteryLow;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, INotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.BatteryQuarter;
                IconContent.Colour = colours.RedDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    return true;
                };
            }
        }

        #endregion
    }
}
