﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PlayerLoader : ScreenWithBeatmapBackground
    {
        private readonly Func<Player> createPlayer;
        private static readonly Vector2 background_blur = new Vector2(15);

        private Player player;

        private Container content;

        private BeatmapMetadataDisplay info;

        private bool hideOverlays;
        public override bool HideOverlaysOnEnter => hideOverlays;

        private Task loadTask;

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
        private void load()
        {
            InternalChild = content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    info = new BeatmapMetadataDisplay(Beatmap.Value)
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
                            visualSettings = new VisualSettings(),
                            new InputSettings()
                        }
                    }
                }
            };

            loadNewPlayer();
        }

        private void playerLoaded(Player player) => info.Loading = false;

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

            loadTask = LoadComponentAsync(player, playerLoaded);
        }

        private void contentIn()
        {
            content.ScaleTo(1, 650, Easing.OutQuint);
            content.FadeInFromZero(400);
        }

        private void contentOut()
        {
            content.ScaleTo(0.7f, 300, Easing.InQuint);
            content.FadeOut(250);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            content.ScaleTo(0.7f);

            contentIn();

            info.Delay(750).FadeIn(500);
            this.Delay(1800).Schedule(pushWhenLoaded);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.RelativePositionAxes = Axes.Both;

            logo.ScaleTo(new Vector2(0.15f), 300, Easing.In);
            logo.MoveTo(new Vector2(0.5f), 300, Easing.In);
            logo.FadeIn(350);

            logo.Delay(resuming ? 0 : 500).MoveToOffset(new Vector2(0, -0.24f), 500, Easing.InOutExpo);
        }

        private ScheduledDelegate pushDebounce;
        private VisualSettings visualSettings;

        private bool readyForPush => player.LoadState == LoadState.Ready && IsHovered && GetContainingInputManager()?.DraggedDrawable == null;

        protected override bool OnHover(HoverEvent e)
        {
            // restore our screen defaults
            InitializeBackgroundElements();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (GetContainingInputManager()?.HoveredDrawables.Contains(visualSettings) == true)
            {
                // show user setting preview
                UpdateBackgroundElements();
            }

            base.OnHoverLost(e);
        }

        protected override void InitializeBackgroundElements()
        {
            Background?.FadeColour(Color4.White, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            Background?.BlurTo(background_blur, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

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
            base.OnSuspending(next);
            cancelLoad();
        }

        public override bool OnExiting(IScreen next)
        {
            content.ScaleTo(0.7f, 150, Easing.InQuint);
            this.FadeOut(150);
            cancelLoad();

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

            public BeatmapMetadataDisplay(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                var metadata = beatmap?.BeatmapInfo?.Metadata ?? new BeatmapMetadata();

                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = new LocalisedString((metadata.TitleUnicode, metadata.Title)),
                                TextSize = 36,
                                Font = @"Exo2.0-MediumItalic",
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new OsuSpriteText
                            {
                                Text = new LocalisedString((metadata.ArtistUnicode, metadata.Artist)),
                                TextSize = 26,
                                Font = @"Exo2.0-MediumItalic",
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
                                TextSize = 26,
                                Font = @"Exo2.0-MediumItalic",
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
                        },
                    }
                };

                Loading = true;
            }
        }
    }
}
