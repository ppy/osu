// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.States;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.PlayerSettings;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PlayerLoader : ScreenWithBeatmapBackground
    {
        private static readonly Vector2 background_blur = new Vector2(15);

        private Player player;

        private BeatmapMetadataDisplay info;

        private bool hideOverlays;
        protected override bool HideOverlaysOnEnter => hideOverlays;

        private Task loadTask;

        public PlayerLoader(Player player)
        {
            this.player = player;

            player.RestartRequested = () =>
            {
                hideOverlays = true;
                ValidForResume = true;
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(info = new BeatmapMetadataDisplay(Beatmap.Value)
            {
                Alpha = 0,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Add(new FillFlowContainer<PlayerSettingsGroup>
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
            });

            loadTask = LoadComponentAsync(player, playerLoaded);
        }

        private void playerLoaded(Player player) => info.Loading = false;

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            contentIn();

            info.Loading = true;

            //we will only be resumed if the player has requested a re-run (see ValidForResume setting above)
            loadTask = LoadComponentAsync(player = new Player
            {
                RestartCount = player.RestartCount + 1,
                RestartRequested = player.RestartRequested,
            }, playerLoaded);

            this.Delay(400).Schedule(pushWhenLoaded);
        }

        private void contentIn()
        {
            Content.ScaleTo(1, 650, Easing.OutQuint);
            Content.FadeInFromZero(400);
        }

        private void contentOut()
        {
            Content.ScaleTo(0.7f, 300, Easing.InQuint);
            Content.FadeOut(250);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Content.ScaleTo(0.7f);

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

        protected override bool OnHover(InputState state)
        {
            // restore our screen defaults
            InitializeBackgroundElements();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (GetContainingInputManager().HoveredDrawables.Contains(visualSettings))
            {
                // show user setting preview
                UpdateBackgroundElements();
            }
            base.OnHoverLost(state);
        }

        protected override void InitializeBackgroundElements()
        {
            Background?.FadeTo(1, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            Background?.BlurTo(background_blur, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        private void pushWhenLoaded()
        {
            if (!IsCurrentScreen) return;

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
                        if (!IsCurrentScreen) return;

                        loadTask = null;

                        //By default, we want to load the player and never be returned to.
                        //Note that this may change if the player we load requested a re-run.
                        ValidForResume = false;

                        if (player.LoadedBeatmapSuccessfully)
                            Push(player);
                        else
                            Exit();
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

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            cancelLoad();
        }

        protected override bool OnExiting(Screen next)
        {
            Content.ScaleTo(0.7f, 150, Easing.InQuint);
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
