// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Match;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeIntro : OsuScreen
    {
        private readonly Room room;
        private readonly PlaylistItem item;

        private FillFlowContainer introContent = null!;
        private Container topPart = null!;
        private Container bottomPart = null!;
        private Container beatmapBackground = null!;
        private Container beatmapTitle = null!;

        private bool beatmapBackgroundLoaded;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        public DailyChallengeIntro(Room room)
        {
            this.room = room;
            item = room.Playlist.Single();

            ValidForResume = false;
        }

        protected override BackgroundScreen CreateBackground() => new DailyChallengeIntroBackgroundScreen(colourProvider);

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                introContent = new FillFlowContainer
                {
                    Alpha = 0f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = topPart = new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Right = 200f },
                                CornerRadius = 10f,
                                Masking = true,
                                Shear = new Vector2(OsuGame.SHEAR, 0f),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = colourProvider.Background3,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Today's Challenge",
                                        Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                        Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                        // Colour = Color4.Black,
                                        Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                    },
                                }
                            },
                        },
                        beatmapBackground = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(500f, 150f),
                            CornerRadius = 20f,
                            BorderColour = colourProvider.Content2,
                            BorderThickness = 3f,
                            Masking = true,
                            Shear = new Vector2(OsuGame.SHEAR, 0f),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background3,
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        },
                        beatmapTitle = new Container
                        {
                            Width = 500f,
                            Margin = new MarginPadding { Right = 160f * OsuGame.SHEAR },
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            CornerRadius = 10f,
                            Masking = true,
                            Shear = new Vector2(OsuGame.SHEAR, 0f),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background3,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = item.Beatmap.GetDisplayString(),
                                    Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                    Font = OsuFont.GetFont(size: 24),
                                },
                            }
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = bottomPart = new Container
                            {
                                Alpha = 0f,
                                AlwaysPresent = true,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Left = 210f },
                                CornerRadius = 10f,
                                Masking = true,
                                Shear = new Vector2(OsuGame.SHEAR, 0f),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = colourProvider.Background3,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Sunday, July 28th",
                                        Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                        Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                        Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                    },
                                }
                            },
                        }
                    }
                }
            };

            LoadComponentAsync(new OnlineBeatmapSetCover(item.Beatmap.BeatmapSet as IBeatmapSetOnlineInfo)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                Scale = new Vector2(1.2f),
                Shear = new Vector2(-OsuGame.SHEAR, 0f),
            }, c =>
            {
                beatmapBackground.Add(c);
                beatmapBackgroundLoaded = true;
                updateAnimationState();
            });
        }

        private bool animationBegan;

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.FadeInFromZero(400, Easing.OutQuint);
            updateAnimationState();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(200, Easing.OutQuint);
            base.OnSuspending(e);
        }

        private void updateAnimationState()
        {
            if (!beatmapBackgroundLoaded || !this.IsCurrentScreen())
                return;

            if (animationBegan)
                return;

            beginAnimation();
            animationBegan = true;
        }

        private void beginAnimation()
        {
            introContent.Show();

            topPart.MoveToX(-500).MoveToX(0, 300, Easing.OutQuint)
                   .FadeInFromZero(400, Easing.OutQuint);

            bottomPart.MoveToX(500).MoveToX(0, 300, Easing.OutQuint)
                      .FadeInFromZero(400, Easing.OutQuint);

            this.Delay(400).Schedule(() =>
            {
                introContent.AutoSizeDuration = 200;
                introContent.AutoSizeEasing = Easing.OutQuint;
            });

            this.Delay(500).Schedule(() => ApplyToBackground(bs => ((RoomBackgroundScreen)bs).SelectedItem.Value = item));

            beatmapBackground.FadeOut().Delay(500)
                             .FadeIn(200, Easing.InQuart);

            beatmapTitle.FadeOut().Delay(500)
                        .FadeIn(200, Easing.InQuart);

            introContent.Delay(1800).FadeOut(200, Easing.OutQuint)
                        .OnComplete(_ =>
                        {
                            if (this.IsCurrentScreen())
                                this.Push(new DailyChallenge(room));
                        });
        }

        private partial class DailyChallengeIntroBackgroundScreen : RoomBackgroundScreen
        {
            private readonly OverlayColourProvider colourProvider;

            public DailyChallengeIntroBackgroundScreen(OverlayColourProvider colourProvider)
                : base(null)
            {
                this.colourProvider = colourProvider;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(new Box
                {
                    Depth = float.MinValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5.Opacity(0.6f),
                });
            }
        }
    }
}
