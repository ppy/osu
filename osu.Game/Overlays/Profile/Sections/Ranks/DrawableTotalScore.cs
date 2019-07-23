// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableTotalScore : OsuHoverContainer
    {
        private const int height = 40;
        private const int corner_radius = 6;

        protected readonly Container InfoContainer;
        protected readonly FillFlowContainer Accuracy;
        protected readonly ScoreInfo Score;

        private readonly Box background;
        private readonly OsuSpriteText performanceValue;
        private readonly OsuSpriteText performance;
        private readonly OsuSpriteText version;
        private readonly FillFlowContainer modsContainer;
        private readonly OsuSpriteText accuracyText;

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        public DrawableTotalScore(ScoreInfo score)
        {
            Score = score;

            Enabled.Value = true; //manually enabled, because we have no action

            RelativeSizeAxes = Axes.X;
            Height = height;
            Masking = true;
            CornerRadius = corner_radius;
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
                        new UpdateableRank(Score.Rank)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(50, 20),
                            FillMode = FillMode.Fit,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(0, 2),
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new ScoreBeatmapMetadataContainer(score.Beatmap)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.BottomLeft,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopLeft,
                                    Spacing = new Vector2(5),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        version = new OsuSpriteText
                                        {
                                            Text = $"{score.Beatmap.Version}",
                                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Regular),
                                        },
                                        new DrawableDate(score.Date, 14),
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
                            Width = 80,
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
                                            Font = OsuFont.GetFont(weight: FontWeight.Bold)
                                        },
                                        performance = new OsuSpriteText
                                        {
                                            Text = "pp",
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                                            Alpha = score.PP.HasValue ? 1 : 0,
                                        }
                                    }
                                }
                            }
                        },
                        InfoContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children = new Drawable[]
                            {
                                Accuracy = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10),
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            Width = 70,
                                            Child = accuracyText = new OsuSpriteText
                                            {
                                                Text = $"{score.Accuracy:P2}",
                                                Font = OsuFont.GetFont(weight: FontWeight.Bold, italics: true),
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

            foreach (Mod mod in score.Mods)
                modsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.4f) });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.GreySeafoam;
            HoverColour = colours.GreySeafoamLight;

            performanceValue.Colour = colours.GreenLight;
            performance.Colour = colours.Green;
            version.Colour = accuracyText.Colour = colours.Yellow;
        }

        private class ScoreBeatmapMetadataContainer : BeatmapMetadataContainer
        {
            public ScoreBeatmapMetadataContainer(BeatmapInfo beatmap)
                : base(beatmap)
            {
            }

            protected override Drawable[] CreateText(BeatmapInfo beatmap) => new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Text = new LocalisedString((
                        $"{beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title} ",
                        $"{beatmap.Metadata.Title ?? beatmap.Metadata.TitleUnicode} ")),
                    Font = OsuFont.GetFont(italics: true)
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Text = "by " + new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                    Font = OsuFont.GetFont(size: 14, italics: true)
                },
            };
        }
    }
}
