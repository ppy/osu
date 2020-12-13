﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PlayerLoader : ScreenWithBeatmapBackground
    {
        protected const float BACKGROUND_BLUR = 15;

        public override bool HideOverlaysOnEnter => hideOverlays;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // Here because IsHovered will not update unless we do so.
        public override bool HandlePositionalInput => true;

        // We show the previous screen status
        protected override UserActivity InitialActivity => null;

        protected override bool PlayResumeSound => false;

        protected BeatmapMetadataDisplay MetadataInfo;

        protected VisualSettings VisualSettings;

        protected Task LoadTask { get; private set; }

        protected Task DisposalTask { get; private set; }

        private bool backgroundBrightnessReduction;

        private readonly BindableDouble volumeAdjustment = new BindableDouble(1);

        protected bool BackgroundBrightnessReduction
        {
            set
            {
                if (value == backgroundBrightnessReduction)
                    return;

                backgroundBrightnessReduction = value;

                Background.FadeColour(OsuColour.Gray(backgroundBrightnessReduction ? 0.8f : 1), 200);
            }
        }

        private bool readyForPush =>
            !playerConsumed
            // don't push unless the player is completely loaded
            && player?.LoadState == LoadState.Ready
            // don't push if the user is hovering one of the panes, unless they are idle.
            && (IsHovered || idleTracker.IsIdle.Value)
            // don't push if the user is dragging a slider or otherwise.
            && inputManager?.DraggedDrawable == null
            // don't push if a focused overlay is visible, like settings.
            && inputManager?.FocusedDrawable == null;

        private readonly Func<Player> createPlayer;

        private Player player;

        /// <summary>
        /// Whether the curent player instance has been consumed via <see cref="consumePlayer"/>.
        /// </summary>
        private bool playerConsumed;

        private LogoTrackingContainer content;

        private bool hideOverlays;

        private InputManager inputManager;

        private IdleTracker idleTracker;

        private ScheduledDelegate scheduledPushPlayer;

        [CanBeNull]
        private EpilepsyWarning epilepsyWarning;

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notificationOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private VolumeOverlay volumeOverlay { get; set; }

        [Resolved]
        private AudioManager audioManager { get; set; }

        public PlayerLoader(Func<Player> createPlayer)
        {
            this.createPlayer = createPlayer;
        }

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            muteWarningShownOnce = sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce);

            InternalChild = (content = new LogoTrackingContainer
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
                new FillFlowContainer<PlayerSettingsGroup>
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Margin = new MarginPadding(25),
                    Children = new PlayerSettingsGroup[]
                    {
                        VisualSettings = new VisualSettings(),
                        new InputSettings()
                    }
                },
                idleTracker = new IdleTracker(750)
            });

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

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            if (epilepsyWarning != null)
                epilepsyWarning.DimmableBackground = Background;
            Beatmap.Value.Track.AddAdjustment(AdjustableProperty.Volume, volumeAdjustment);

            content.ScaleTo(0.7f);
            Background?.FadeColour(Color4.White, 800, Easing.OutQuint);

            contentIn();

            MetadataInfo.Delay(750).FadeIn(500);

            // after an initial delay, start the debounced load check.
            // this will continue to execute even after resuming back on restart.
            Scheduler.Add(new ScheduledDelegate(pushWhenLoaded, 1800, 0));

            showMuteWarningIfNeeded();
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            // prepare for a retry.
            player = null;
            playerConsumed = false;
            cancelLoad();

            contentIn();
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            BackgroundBrightnessReduction = false;

            // we're moving to player, so a period of silence is upcoming.
            // stop the track before removing adjustment to avoid a volume spike.
            Beatmap.Value.Track.Stop();
            Beatmap.Value.Track.RemoveAdjustment(AdjustableProperty.Volume, volumeAdjustment);
        }

        public override bool OnExiting(IScreen next)
        {
            cancelLoad();

            content.ScaleTo(0.7f, 150, Easing.InQuint);
            this.FadeOut(150);

            Background.EnableUserDim.Value = false;
            BackgroundBrightnessReduction = false;
            Beatmap.Value.Track.RemoveAdjustment(AdjustableProperty.Volume, volumeAdjustment);

            return base.OnExiting(next);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            const double duration = 300;

            if (!resuming) logo.MoveTo(new Vector2(0.5f), duration, Easing.In);

            logo.ScaleTo(new Vector2(0.15f), duration, Easing.In);
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
                Background.EnableUserDim.Value = true;
                Background.BlurAmount.Value = 0;

                BackgroundBrightnessReduction = false;
            }
            else
            {
                // Returns background dim and blur to the values specified by PlayerLoader.
                Background.EnableUserDim.Value = false;
                Background.BlurAmount.Value = BACKGROUND_BLUR;

                BackgroundBrightnessReduction = true;
            }
        }

        private Player consumePlayer()
        {
            Debug.Assert(!playerConsumed);

            playerConsumed = true;
            return player;
        }

        private void prepareNewPlayer()
        {
            if (!this.IsCurrentScreen())
                return;

            var restartCount = player?.RestartCount + 1 ?? 0;

            player = createPlayer();
            player.RestartCount = restartCount;
            player.RestartRequested = restartRequested;

            LoadTask = LoadComponentAsync(player, _ => MetadataInfo.Loading = false);
        }

        private void restartRequested()
        {
            hideOverlays = true;
            ValidForResume = true;
        }

        private void contentIn()
        {
            MetadataInfo.Loading = true;

            content.FadeInFromZero(400);
            content.ScaleTo(1, 650, Easing.OutQuint).Then().Schedule(prepareNewPlayer);
        }

        private void contentOut()
        {
            // Ensure the logo is no longer tracking before we scale the content
            content.StopTracking();

            content.ScaleTo(0.7f, 300, Easing.InQuint);
            content.FadeOut(250);
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

                contentOut();

                TransformSequence<PlayerLoader> pushSequence = this.Delay(250);

                // only show if the warning was created (i.e. the beatmap needs it)
                // and this is not a restart of the map (the warning expires after first load).
                if (epilepsyWarning?.IsAlive == true)
                {
                    const double epilepsy_display_length = 3000;

                    pushSequence
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
                DisposalTask = LoadTask?.ContinueWith(_ => player?.Dispose());
            }
        }

        #endregion

        #region Mute warning

        private Bindable<bool> muteWarningShownOnce;

        private void showMuteWarningIfNeeded()
        {
            if (!muteWarningShownOnce.Value)
            {
                // Checks if the notification has not been shown yet and also if master volume is muted, track/music volume is muted or if the whole game is muted.
                if (volumeOverlay?.IsMuted.Value == true || audioManager.Volume.Value <= audioManager.Volume.MinValue || audioManager.VolumeTrack.Value <= audioManager.VolumeTrack.MinValue)
                {
                    notificationOverlay?.Post(new MutedNotification());
                    muteWarningShownOnce.Value = true;
                }
            }
        }

        private class MutedNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public MutedNotification()
            {
                Text = "Your music volume is set to 0%! Click here to restore it.";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audioManager, NotificationOverlay notificationOverlay, VolumeOverlay volumeOverlay)
            {
                Icon = FontAwesome.Solid.VolumeMute;
                IconBackgound.Colour = colours.RedDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();

                    volumeOverlay.IsMuted.Value = false;
                    audioManager.Volume.SetDefault();
                    audioManager.VolumeTrack.SetDefault();

                    return true;
                };
            }
        }

        #endregion
    }
}
