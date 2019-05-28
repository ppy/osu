// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections
{
    public class DrawableScore : Container
    {
        private Color4 idleBackgroundColour;
        private Color4 hoveredBackgroundColour;
        private const int duration = 200;
        private readonly Box background;
        private readonly OsuSpriteText performanceValue;
        private readonly OsuSpriteText performance;
        private readonly OsuSpriteText version;
        private readonly ScoreInfo score;
        private readonly FillFlowContainer modsContainer;
        private readonly Container infoContainer;
        private readonly double? weight;
        private readonly OsuSpriteText accuracyText;
        private readonly FillFlowContainer accuracy;

        public DrawableScore(ScoreInfo score, double? weight = null)
        {
            this.score = score;
            this.weight = weight;

            RelativeSizeAxes = Axes.X;
            Height = 50;
            Masking = true;
            CornerRadius = 10;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 10 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new DrawableRank(score.Rank)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(60, 30),
                            FillMode = FillMode.Fit,
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                new BeatmapLink(score.Beatmap, BeatmapLinkType.TitleAuthor)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.BottomLeft,
                                    Margin = new MarginPadding { Bottom = 2 },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopLeft,
                                    Margin = new MarginPadding { Top = 2 },
                                    Spacing = new Vector2(5),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        version = new OsuSpriteText
                                        {
                                            Text = $"{score.Beatmap.Version}",
                                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.Regular),
                                        },
                                        new DrawableDate(score.Date),
                                    }
                                },
                            }
                        }
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 100,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(1, 0.5f),
                                    Colour = Color4.Black.Opacity(0.5f),
                                    Shear = new Vector2(-0.45f, 0),
                                    EdgeSmoothness = new Vector2(2, 0),
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Y,
                                    Size = new Vector2(1, -0.5f),
                                    Position = new Vector2(0, 1),
                                    Colour = Color4.Black.Opacity(0.5f),
                                    Shear = new Vector2(0.45f, 0),
                                    EdgeSmoothness = new Vector2(2, 0),
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        performanceValue = new OsuSpriteText
                                        {
                                            Text = score.PP.HasValue ? $"{score.PP:0}" : "-",
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold)
                                        },
                                        performance = new OsuSpriteText
                                        {
                                            Text = "pp",
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold),
                                            Alpha = score.PP.HasValue ? 1 : 0,
                                        }
                                    }
                                }
                            }
                        },
                        infoContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children = new Drawable[]
                            {
                                accuracy = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(30),
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            Width = 70,
                                            Child = accuracyText = new OsuSpriteText
                                            {
                                                Text = $"{score.Accuracy:P2}",
                                                Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold, italics: true, fixedWidth: true),
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        modsContainer = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2),
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            idleBackgroundColour = background.Colour = colors.GreySeafoam;
            hoveredBackgroundColour = colors.GreySeafoamLight;
            performanceValue.Colour = colors.GreenLight;
            performance.Colour = colors.Green;
            version.Colour = colors.Yellow;

            foreach (Mod mod in score.Mods)
                modsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.5f) });

            accuracyText.Colour = colors.Yellow;

            if (weight.HasValue)
            {
                accuracy.Origin = Anchor.BottomLeft;
                accuracy.Margin = new MarginPadding { Bottom = 2 };

                accuracy.Add(new Container
                {
                    AutoSizeAxes = Axes.Y,
                    Width = 50,
                    Child = new OsuSpriteText
                    {
                        Text = $"{score.PP * weight:0}pp",
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold, italics: true, fixedWidth: true),
                    }
                });
                infoContainer.Add(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.TopLeft,
                    Text = $"weighted {weight:P0}",
                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.SemiBold),
                    Margin = new MarginPadding { Top = 2 },
                });
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(hoveredBackgroundColour, duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(idleBackgroundColour, duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
