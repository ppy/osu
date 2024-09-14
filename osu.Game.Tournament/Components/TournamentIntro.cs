// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentIntro : CompositeDrawable, IAnimation
    {
        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private readonly RoundBeatmap map;
        private readonly string mod;
        private readonly TeamColour colour;
        private readonly ColourInfo themeColour;
        private readonly TrapInfo? mapTrap;
        private readonly ColourInfo modColour;

        private Container introContent = null!;
        private Container topTitleDisplay = null!;
        private Container secondDisplay = null!;
        private Container beatmapBackground = null!;
        private FillFlowContainer authorDisplay = null!;
        private Container trapDisplay = null!;
        private Box flash = null!;
        private EmptyBox dummyBackground = null!;
        private OsuSpriteText modText = null!;

        private FillFlowContainer beatmapContent = null!;

        private Container titleContainer = null!;

        private readonly OverlayColourProvider colourProvider;

        public event Action? OnAnimationComplete;
        public AnimationStatus Status { get; private set; } = AnimationStatus.Loading;

        public TournamentIntro(RoundBeatmap map, TeamColour colour = TeamColour.Neutral, TrapInfo? trap = null)
        {
            this.map = map;
            this.colour = colour;
            colourProvider = new OverlayColourProvider(colour == TeamColour.Red ? OverlayColourScheme.Pink : colour == TeamColour.Blue ? OverlayColourScheme.Blue : OverlayColourScheme.Plum);
            themeColour = colour == TeamColour.Red ? new OsuColour().Pink1 : colour == TeamColour.Blue ? new OsuColour().Sky : Color4.White;
            mapTrap = trap;
            mod = map.Mods + map.ModIndex;

            switch (map.Mods)
            {
                case "HR":
                    modColour = Color4Extensions.FromHex("#f76363");
                    break;

                case "FM":
                    modColour = Color4Extensions.FromHex("#24eecb");
                    break;

                case "NM":
                    modColour = Color4Extensions.FromHex("#ffdb75");
                    break;

                case "DT":
                    modColour = Color4Extensions.FromHex("#66ccff");
                    break;

                case "HD":
                    modColour = Color4Extensions.FromHex("#fdc300");
                    break;

                case "EX":
                    modColour = Color4Extensions.FromHex("#ffa500");
                    break;

                case "TB":
                    modColour = Color4.Yellow;
                    break;

                default:
                    modColour = Color4.White;
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float horizontal_info_size = 500f;

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
                                secondDisplay = new Container
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
                                                new StarRatingDisplay(new StarDifficulty(map.Beatmap?.StarRating ?? 0, 0))
                                                {
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Margin = new MarginPadding(5),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                }
                                            }
                                        },
                                        authorDisplay = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                            Direction = FillDirection.Horizontal,
                                            Padding = new MarginPadding(5f),
                                            Alpha = 0,
                                            Children = new Drawable[]
                                            {
                                                new SpriteIcon
                                                {
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
                                                    Icon = FontAwesome.Solid.Check,
                                                    Size = new Vector2(20),
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Colour = themeColour,
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"{(colour == TeamColour.Red ? "Red" : colour == TeamColour.Blue ? "Blue" : "Smoke")} Team Picked",
                                                    Font = OsuFont.GetFont(size: 16, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Colour = themeColour,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
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

            if (mapTrap != null)
            {
                AddInternal(trapDisplay = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = horizontal_info_size,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 10f,
                    Masking = true,
                    Shear = new Vector2(OsuGame.SHEAR, 0f),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new TruncatingSpriteText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                            MaxWidth = horizontal_info_size,
                            Text = "Also triggered the trap:",
                            Padding = new MarginPadding { Horizontal = 5f },
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 26, weight: FontWeight.SemiBold),
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            X = 20,
                            Y = 15,
                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                            Icon = mapTrap.Icon,
                            Colour = mapTrap.IconColor,
                            Size = new Vector2(32),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            X = 55,
                            Width = 0.9f,
                            Y = 15,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding(5f),
                            Children = new Drawable[]
                            {
                                new TruncatingSpriteText
                                {
                                    Text = mapTrap.Name,
                                    Font = OsuFont.GetFont(typeface: Typeface.HarmonyOSSans, size: 30, weight: FontWeight.Bold),
                                    MaxWidth = horizontal_info_size,
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                new TruncatingSpriteText
                                {
                                    Text = mapTrap.Description,
                                    Font = OsuFont.GetFont(typeface: Typeface.HarmonyOSSans, size: 20, weight: FontWeight.Regular),
                                    MaxWidth = horizontal_info_size,
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                            }
                        },
                    }
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
            });
        }

        public void Fire()
        {
            beginAnimation();
        }

        private void beginAnimation()
        {
            this.FadeInFromZero(500, Easing.OutExpo);

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
                    .Then().FadeColour(modColour, 500, Easing.OutQuint);

                secondDisplay.MoveToY(y_offset_start)
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

                        using (BeginDelayedSequence(200))
                            authorDisplay.FadeInFromZero(200, Easing.InQuad);

                        using (BeginDelayedSequence(400))
                            flash.FadeOutFromOne(5000, Easing.OutQuint);
                    }

                    if (mapTrap != null)
                    {
                        using (BeginDelayedSequence(1000))
                        {
                            trapDisplay.FadeInFromZero(800, Easing.InQuad);
                            trapDisplay.MoveToOffset(new Vector2(0, 175), 600, Easing.OutQuint);
                            beatmapContent.MoveToOffset(new Vector2(0, -75), 600, Easing.OutQuint);
                            trapDisplay.Delay(100).ScaleTo(1.3f, 1500, Easing.OutQuint);
                        }
                    }
                }

                using (BeginDelayedSequence(6000))
                {
                    this.FadeOutFromOne(3000, Easing.OutExpo).Then().Finally(_ =>
                    {
                        Status = AnimationStatus.Complete;
                        OnAnimationComplete?.Invoke();
                        Expire();
                    });
                }
            }
        }
    }
}
