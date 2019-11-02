// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PlayerLoader : ScreenWithBeatmapBackground
    {
        protected const float BACKGROUND_BLUR = 15;

        private readonly Func<Player> createPlayer;

        private Player player;

        private LogoTrackingContainer content;

        private BeatmapMetadataDisplay info;

        private bool hideOverlays;
        public override bool HideOverlaysOnEnter => hideOverlays;

        protected override UserActivity InitialActivity => null; //shows the previous screen status

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        protected override bool PlayResumeSound => false;

        private Task loadTask;

        private InputManager inputManager;
        private IdleTracker idleTracker;

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notificationOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private VolumeOverlay volumeOverlay { get; set; }

        [Resolved]
        private AudioManager audioManager { get; set; }

        private Bindable<bool> muteWarningShownOnce;

        public PlayerLoader(Func<Player> createPlayer)
        {
            this.createPlayer = createPlayer;
        }

        private void restartRequested()
        {
            hideOverlays = true;
            ValidForResume = true;
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
                info = new BeatmapMetadataDisplay(Beatmap.Value, Mods.Value, content.LogoFacade)
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

            loadNewPlayer();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            if (!muteWarningShownOnce.Value)
            {
                //Checks if the notification has not been shown yet and also if master volume is muted, track/music volume is muted or if the whole game is muted.
                if (volumeOverlay?.IsMuted.Value == true || audioManager.Volume.Value <= audioManager.Volume.MinValue || audioManager.VolumeTrack.Value <= audioManager.VolumeTrack.MinValue)
                {
                    notificationOverlay?.Post(new MutedNotification());
                    muteWarningShownOnce.Value = true;
                }
            }
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            contentIn();

            info.Loading = true;

            //we will only be resumed if the player has requested a re-run (see ValidForResume setting above)
            loadNewPlayer();

            this.Delay(400).Schedule(pushWhenLoaded);
        }

        private void loadNewPlayer()
        {
            var restartCount = player?.RestartCount + 1 ?? 0;

            player = createPlayer();
            player.RestartCount = restartCount;
            player.RestartRequested = restartRequested;

            loadTask = LoadComponentAsync(player, _ => info.Loading = false);
        }

        private void contentIn()
        {
            content.ScaleTo(1, 650, Easing.OutQuint);
            content.FadeInFromZero(400);
        }

        private void contentOut()
        {
            // Ensure the logo is no longer tracking before we scale the content
            content.StopTracking();

            content.ScaleTo(0.7f, 300, Easing.InQuint);
            content.FadeOut(250);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            content.ScaleTo(0.7f);
            Background?.FadeColour(Color4.White, 800, Easing.OutQuint);

            contentIn();

            info.Delay(750).FadeIn(500);
            this.Delay(1800).Schedule(pushWhenLoaded);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            const double duration = 300;

            if (!resuming)
            {
                logo.MoveTo(new Vector2(0.5f), duration, Easing.In);
            }

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

        private ScheduledDelegate pushDebounce;
        protected VisualSettings VisualSettings;

        // Here because IsHovered will not update unless we do so.
        public override bool HandlePositionalInput => true;

        private bool readyForPush => player.LoadState == LoadState.Ready && (IsHovered || idleTracker.IsIdle.Value) && inputManager?.DraggedDrawable == null;

        private void pushWhenLoaded()
        {
            if (!this.IsCurrentScreen()) return;

            try
            {
                if (!readyForPush)
                {
                    // as the pushDebounce below has a delay, we need to keep checking and cancel a future debounce
                    // if we become unready for push during the delay.
                    cancelLoad();
                    return;
                }

                if (pushDebounce != null)
                    return;

                pushDebounce = Scheduler.AddDelayed(() =>
                {
                    contentOut();

                    this.Delay(250).Schedule(() =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        loadTask = null;

                        //By default, we want to load the player and never be returned to.
                        //Note that this may change if the player we load requested a re-run.
                        ValidForResume = false;

                        if (player.LoadedBeatmapSuccessfully)
                            this.Push(player);
                        else
                            this.Exit();
                    });
                }, 500);
            }
            finally
            {
                Schedule(pushWhenLoaded);
            }
        }

        private void cancelLoad()
        {
            pushDebounce?.Cancel();
            pushDebounce = null;
        }

        public override void OnSuspending(IScreen next)
        {
            BackgroundBrightnessReduction = false;
            base.OnSuspending(next);
            cancelLoad();
        }

        public override bool OnExiting(IScreen next)
        {
            content.ScaleTo(0.7f, 150, Easing.InQuint);
            this.FadeOut(150);
            cancelLoad();

            Background.EnableUserDim.Value = false;
            BackgroundBrightnessReduction = false;

            return base.OnExiting(next);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
            {
                // if the player never got pushed, we should explicitly dispose it.
                loadTask?.ContinueWith(_ => player.Dispose());
            }
        }

        private bool backgroundBrightnessReduction;

        protected bool BackgroundBrightnessReduction
        {
            get => backgroundBrightnessReduction;
            set
            {
                if (value == backgroundBrightnessReduction)
                    return;

                backgroundBrightnessReduction = value;

                Background.FadeColour(OsuColour.Gray(backgroundBrightnessReduction ? 0.8f : 1), 200);
            }
        }

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

        private class BeatmapMetadataDisplay : Container
        {
            private class MetadataLine : Container
            {
                public MetadataLine(string left, string right)
                {
                    AutoSizeAxes = Axes.Both;
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight,
                            Margin = new MarginPadding { Right = 5 },
                            Colour = OsuColour.Gray(0.8f),
                            Text = left,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Left = 5 },
                            Text = string.IsNullOrEmpty(right) ? @"-" : right,
                        }
                    };
                }
            }

            private readonly WorkingBeatmap beatmap;
            private readonly IReadOnlyList<Mod> mods;
            private readonly Drawable facade;
            private LoadingAnimation loading;
            private Sprite backgroundSprite;

            public bool Loading
            {
                set
                {
                    if (value)
                    {
                        loading.Show();
                        backgroundSprite.FadeColour(OsuColour.Gray(0.5f), 400, Easing.OutQuint);
                    }
                    else
                    {
                        loading.Hide();
                        backgroundSprite.FadeColour(Color4.White, 400, Easing.OutQuint);
                    }
                }
            }

            public BeatmapMetadataDisplay(WorkingBeatmap beatmap, IReadOnlyList<Mod> mods, Drawable facade)
            {
                this.beatmap = beatmap;
                this.mods = mods;
                this.facade = facade;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                var metadata = beatmap.BeatmapInfo?.Metadata ?? new BeatmapMetadata();

                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        Children = new[]
                        {
                            facade.With(d =>
                            {
                                d.Anchor = Anchor.TopCentre;
                                d.Origin = Anchor.TopCentre;
                            }),
                            new OsuSpriteText
                            {
                                Text = new LocalisedString((metadata.TitleUnicode, metadata.Title)),
                                Font = OsuFont.GetFont(size: 36, italics: true),
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 15 },
                            },
                            new OsuSpriteText
                            {
                                Text = new LocalisedString((metadata.ArtistUnicode, metadata.Artist)),
                                Font = OsuFont.GetFont(size: 26, italics: true),
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new Container
                            {
                                Size = new Vector2(300, 60),
                                Margin = new MarginPadding(10),
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                CornerRadius = 10,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    backgroundSprite = new Sprite
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Texture = beatmap?.Background,
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        FillMode = FillMode.Fill,
                                    },
                                    loading = new LoadingAnimation { Scale = new Vector2(1.3f) }
                                }
                            },
                            new OsuSpriteText
                            {
                                Text = beatmap?.BeatmapInfo?.Version,
                                Font = OsuFont.GetFont(size: 26, italics: true),
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Margin = new MarginPadding
                                {
                                    Bottom = 40
                                },
                            },
                            new MetadataLine("Source", metadata.Source)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new MetadataLine("Mapper", metadata.AuthorString)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new ModDisplay
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Top = 20 },
                                Current = { Value = mods }
                            }
                        },
                    }
                };

                Loading = true;
            }
        }

        private class MutedNotification : SimpleNotification
        {
            public MutedNotification()
            {
                Text = "Your music volume is set to 0%! Click here to restore it.";
            }

            public override bool IsImportant => true;

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
    }
}
