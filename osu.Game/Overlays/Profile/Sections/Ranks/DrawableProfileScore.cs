// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring.Drawables;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public partial class DrawableProfileScore : CompositeDrawable
    {
        private const int height = 40;
        private const int performance_width = 100;

        private const float performance_background_shear = 0.45f;

        protected readonly SoloScoreInfo Score;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public DrawableProfileScore(SoloScoreInfo score)
        {
            Score = score;

            RelativeSizeAxes = Axes.X;
            Height = height;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var ruleset = rulesets.GetRuleset(Score.RulesetID)?.CreateInstance() ?? throw new InvalidOperationException($"Ruleset with ID of {Score.RulesetID} not found locally");

            AddInternal(new ProfileItemContainer
            {
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = 20, Right = performance_width },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(10, 0),
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
                                            new ScoreBeatmapMetadataContainer(Score.Beatmap.AsNonNull()),
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(15, 0),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = $"{Score.Beatmap.AsNonNull().DifficultyName}",
                                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                                                        Colour = colours.Yellow
                                                    },
                                                    new DrawableDate(Score.EndedAt, 12)
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
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(15),
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        AutoSizeAxes = Axes.X,
                                        RelativeSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Child = CreateRightContent()
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(2),
                                        Children = Score.Mods.Select(m => m.ToMod(ruleset)).AsOrdered().Select(mod => new ModIcon(mod)
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
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                Colour = colourProvider.Background3,
                                Shear = new Vector2(-performance_background_shear, 0),
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            new Box
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Y,
                                Height = -0.5f,
                                Position = new Vector2(0, 1),
                                Colour = colourProvider.Background3,
                                Shear = new Vector2(performance_background_shear, 0),
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Vertical = 5,
                                    Left = 30,
                                    Right = 20
                                },
                                Child = createDrawablePerformance().With(d =>
                                {
                                    d.Anchor = Anchor.Centre;
                                    d.Origin = Anchor.Centre;
                                })
                            }
                        }
                    }
                }
            });
        }

        protected virtual Drawable CreateRightContent() => CreateDrawableAccuracy();

        protected Drawable CreateDrawableAccuracy() => new Container
        {
            Width = 65,
            RelativeSizeAxes = Axes.Y,
            Child = new OsuSpriteText
            {
                Text = Score.Accuracy.FormatAccuracy(),
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                Colour = colours.Yellow,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft
            }
        };

        private Drawable createDrawablePerformance()
        {
            if (!Score.PP.HasValue)
            {
                if (Score.Beatmap?.Status.GrantsPerformancePoints() == true)
                    return new UnprocessedPerformancePointsPlaceholder { Size = new Vector2(16), Colour = colourProvider.Highlight1 };

                return new OsuSpriteText
                {
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                    Text = "-",
                    Colour = colourProvider.Highlight1
                };
            }

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

        private partial class ScoreBeatmapMetadataContainer : BeatmapMetadataContainer
        {
            public ScoreBeatmapMetadataContainer(IBeatmapInfo beatmapInfo)
                : base(beatmapInfo)
            {
            }

            protected override Drawable[] CreateText(IBeatmapInfo beatmapInfo)
            {
                var metadata = beatmapInfo.Metadata;

                return new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = new RomanisableString(metadata.TitleUnicode, metadata.Title),
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold, italics: true)
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = " by ",
                        Font = OsuFont.GetFont(size: 12, italics: true)
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = new RomanisableString(metadata.ArtistUnicode, metadata.Artist),
                        Font = OsuFont.GetFont(size: 12, italics: true)
                    },
                };
            }
        }
    }
}
