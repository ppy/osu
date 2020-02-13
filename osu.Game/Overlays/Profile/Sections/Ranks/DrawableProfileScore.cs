// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableProfileScore : CompositeDrawable
    {
        private const int height = 40;
        private const int performance_width = 80;

        private const float performance_background_shear = 0.45f;
        private static readonly float performance_background_width = performance_width + (height / 4f * MathF.Tan(performance_background_shear));

        protected readonly ScoreInfo Score;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public DrawableProfileScore(ScoreInfo score)
        {
            Score = score;

            RelativeSizeAxes = Axes.X;
            Height = height;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new ProfileItemContainer
            {
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = 10, Right = performance_width + 30 },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(8, 0),
                                Children = new Drawable[]
                                {
                                    new UpdateableRank(Score.Rank)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(50, 20),
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 2),
                                        Children = new Drawable[]
                                        {
                                            new ScoreBeatmapMetadataContainer(Score.Beatmap),
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(5, 0),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = $"{Score.Beatmap.Version}",
                                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                                                        Colour = colours.Yellow
                                                    },
                                                    new DrawableDate(Score.Date, 12)
                                                    {
                                                        Colour = colourProvider.Foreground1
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(15),
                                Children = new[]
                                {
                                    CreateRightContent().With(c =>
                                    {
                                        c.Anchor = Anchor.CentreRight;
                                        c.Origin = Anchor.CentreRight;
                                    }),
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(2),
                                        Children = Score.Mods.Select(mod => new ModIcon(mod)
                                        {
                                            Scale = new Vector2(0.35f)
                                        }).ToList(),
                                    }
                                }
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = performance_width,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Children = new[]
                        {
                            new Box
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Y,
                                Width = performance_background_width,
                                Height = 0.5f,
                                Colour = colourProvider.Background4,
                                Shear = new Vector2(-performance_background_shear, 0),
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            new Box
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Y,
                                RelativePositionAxes = Axes.Y,
                                Width = performance_background_width,
                                Height = -0.5f,
                                Position = new Vector2(0, 1),
                                Colour = colourProvider.Background4,
                                Shear = new Vector2(performance_background_shear, 0),
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            createDrawablePerformance().With(d =>
                            {
                                d.Anchor = Anchor.Centre;
                                d.Origin = Anchor.Centre;
                            })
                        }
                    }
                }
            });
        }

        [NotNull]
        protected virtual Drawable CreateRightContent() => CreateDrawableAccuracy();

        protected OsuSpriteText CreateDrawableAccuracy() => new OsuSpriteText
        {
            Text = Score.DisplayAccuracy,
            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
            Colour = colours.Yellow,
        };

        private Drawable createDrawablePerformance()
        {
            if (Score.PP.HasValue)
            {
                return new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold),
                            Text = $"{Score.PP:0}",
                            Colour = colourProvider.Highlight1
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                            Text = "pp",
                            Colour = colourProvider.Light3
                        }
                    }
                };
            }

            return new OsuSpriteText
            {
                Font = OsuFont.GetFont(weight: FontWeight.Bold),
                Text = "-",
                Colour = colourProvider.Highlight1
            };
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
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold, italics: true)
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Text = "by " + new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                    Font = OsuFont.GetFont(size: 12, italics: true)
                },
            };
        }
    }
}
