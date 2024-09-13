// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentIntro : CompositeDrawable
    {
        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private RoundBeatmap map = null!;
        private string mod = null!;
        private ColourInfo color;

        private Container introContent = null!;
        private Container topTitleDisplay = null!;
        private Container bottomDateDisplay = null!;
        private Container beatmapBackground = null!;
        private Box flash = null!;
        private EmptyBox dummyBackground = null!;
        private OsuSpriteText modText = null!;

        private FillFlowContainer beatmapContent = null!;

        private Container titleContainer = null!;

        private bool beatmapBackgroundLoaded;
        private static bool isAnimationRunning = false;

        private bool animationBegan;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        public TournamentIntro(RoundBeatmap map)
        {
            this.map = map;
            mod = map.Mods + map.ModIndex;

            switch (map.Mods)
            {
                case "HR":
                    color = Color4Extensions.FromHex("#f76363");
                    break;

                case "FM":
                    color = Color4Extensions.FromHex("#24eecb");
                    break;

                case "NM":
                    color = Color4Extensions.FromHex("#ffdb75");
                    break;

                case "DT":
                    color = Color4Extensions.FromHex("#66ccff");
                    break;

                case "HD":
                    color = Color4Extensions.FromHex("#fdc300");
                    break;

                case "EX":
                    color = Color4Extensions.FromHex("#ffa500");
                    break;

                case "TB":
                    color = Color4.Yellow;
                    break;

                default:
                    color = Color4.White;
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache difficultyCache)
        {
            const float horizontal_info_size = 500f;

            StarRatingDisplay starRatingDisplay;

            InternalChildren = new Drawable[]
            {
                dummyBackground = new EmptyBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = 1366,
                    Height = (int)(1366 * 9f / 16f),
                    Colour = Color4.Black.Opacity(0.6f),
                    Alpha = 0f,
                },
                introContent = new Container
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = new Vector2(OsuGame.SHEAR, 0f),
                    Children = new Drawable[]
                    {
                        titleContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                topTitleDisplay = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    X = -10,
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
                                            Text = "You've picked...",
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                        },
                                    }
                                },
                                bottomDateDisplay = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    X = 10,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        modText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = $"...{mod}:",
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Font = OsuFont.GetFont(size: 45, weight: FontWeight.SemiBold, typeface: Typeface.TorusAlternate),
                                            Colour = colourProvider.Background3,
                                        },
                                    }
                                },
                            }
                        },
                        beatmapContent = new FillFlowContainer
                        {
                            AlwaysPresent = true, // so we can get the size ahead of time
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Scale = new Vector2(0.001f),
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                beatmapBackground = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Size = new Vector2(horizontal_info_size, 150f),
                                    CornerRadius = 20f,
                                    BorderColour = colourProvider.Content2,
                                    BorderThickness = 3f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        flash = new Box
                                        {
                                            Colour = Color4.White,
                                            Blending = BlendingParameters.Additive,
                                            RelativeSizeAxes = Axes.Both,
                                            Depth = float.MinValue,
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Width = horizontal_info_size,
                                    AutoSizeAxes = Axes.Y,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Direction = FillDirection.Vertical,
                                            Padding = new MarginPadding(5f),
                                            Children = new Drawable[]
                                            {
                                                new TruncatingSpriteText
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    MaxWidth = horizontal_info_size,
                                                    Text = map.Beatmap != null ? map.Beatmap.Metadata.GetDisplayTitleRomanisable(false) : "This beatmap!",
                                                    Padding = new MarginPadding { Horizontal = 5f },
                                                    Font = OsuFont.GetFont(size: 26),
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"Difficulty: {(map.Beatmap != null ? map.Beatmap.DifficultyName : "A Random Difficulty")}",
                                                    Font = OsuFont.GetFont(size: 20, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"by {(map.Beatmap != null ? map.Beatmap.Metadata.Author.Username : "A Random Mapper")}",
                                                    Font = OsuFont.GetFont(size: 16, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                },
                                                starRatingDisplay = new StarRatingDisplay(new StarDifficulty(map.Beatmap?.StarRating ?? 0, 0))
                                                {
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Margin = new MarginPadding(5),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                }
                                            }
                                        },
                                    }
                                },
                            }
                        },
                    }
                }
            };

            LoadComponentAsync(new OnlineBeatmapSetCover(map.Beatmap)
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
                sceneManager?.HideShowChat(400);
            });
            this.FadeInFromZero(500, Easing.OutExpo);
            updateAnimationState();
        }

        private void updateAnimationState()
        {
            if (!beatmapBackgroundLoaded)
                return;

            if (animationBegan)
                return;

            beginAnimation();
            animationBegan = true;
        }

        private void beginAnimation()
        {
            using (BeginDelayedSequence(1500))
            {
                introContent.Show();

                const float y_offset_start = 260;
                const float y_offset_end = 20;

                dummyBackground
                    .FadeInFromZero(300, Easing.OutQuint);

                topTitleDisplay
                    .FadeInFromZero(400, Easing.OutQuint);

                topTitleDisplay.MoveToY(-y_offset_start)
                               .MoveToY(-y_offset_end, 300, Easing.OutQuint)
                               .Then()
                               .MoveToY(0, 4000);

                modText.Delay(200)
                    .Then().FadeColour(color, 500, Easing.OutQuint);

                bottomDateDisplay.MoveToY(y_offset_start)
                                 .MoveToY(y_offset_end, 300, Easing.OutQuint)
                                 .Then()
                                 .MoveToY(0, 4000);

                using (BeginDelayedSequence(1000))
                {
                    beatmapContent
                        .ScaleTo(3)
                        .ScaleTo(1.15f, 500, Easing.In)
                        .Then()
                        .ScaleTo(1.3f, 2000, Easing.OutCubic);

                    using (BeginDelayedSequence(100))
                    {
                        titleContainer
                            .ScaleTo(0.4f, 400, Easing.In)
                            .FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(240))
                    {
                        beatmapContent.FadeInFromZero(280, Easing.InQuad);

                        using (BeginDelayedSequence(400))
                            flash.FadeOutFromOne(5000, Easing.OutQuint);
                    }
                }

                using (BeginDelayedSequence(6000))
                {
                    this.FadeOutFromOne(3000, Easing.OutExpo);
                }
            }
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
