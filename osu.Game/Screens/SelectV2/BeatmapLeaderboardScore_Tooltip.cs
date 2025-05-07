// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapLeaderboardScore
    {
        public partial class LeaderboardScoreTooltip : VisibilityContainer, ITooltip<ScoreInfo>
        {
            private const float spacing = 20f;

            private DateAndStatisticsPanel dateAndStatistics = null!;
            private ModsPanel modsPanel = null!;
            private TotalScoreRankPanel totalScoreRankPanel = null!;

            [Cached]
            private readonly OverlayColourProvider colourProvider;

            public LeaderboardScoreTooltip(OverlayColourProvider colourProvider)
            {
                this.colourProvider = colourProvider;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Width = 170;
                AutoSizeAxes = Axes.Y;

                InternalChild = new ReverseChildIDFillFlowContainer<Drawable>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, -spacing),
                    Children = new Drawable[]
                    {
                        dateAndStatistics = new DateAndStatisticsPanel(),
                        modsPanel = new ModsPanel(),
                        totalScoreRankPanel = new TotalScoreRankPanel(),
                    },
                };
            }

            private ScoreInfo? lastContent;

            public void SetContent(ScoreInfo content)
            {
                if (lastContent != null && lastContent.Equals(content))
                    return;

                dateAndStatistics.Score = content;
                modsPanel.Score = content;
                totalScoreRankPanel.Score = content;
                lastContent = content;
            }

            protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);
            protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);
            public void Move(Vector2 pos) => Position = pos;

            private partial class DateAndStatisticsPanel : CompositeDrawable
            {
                private OsuSpriteText absoluteDate = null!;
                private DrawableDate relativeDate = null!;
                private FillFlowContainer statistics = null!;

                [Resolved]
                private OsuColour colours { get; set; } = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                public ScoreInfo Score
                {
                    set
                    {
                        absoluteDate.Text = value.Date.ToLocalisableString(@"dd MMMM yyyy h:mm tt");
                        relativeDate.Date = value.Date;

                        var judgementsStatistics = value.GetStatisticsForDisplay().Select(s =>
                            new StatisticRow(s.DisplayName.ToUpper(), colours.ForHitResult(s.Result), s.Count.ToLocalisableString("N0")));

                        double multiplier = 1.0;

                        foreach (var mod in value.Mods)
                            multiplier *= mod.ScoreMultiplier;

                        var generalStatistics = new[]
                        {
                            new StatisticRow("Score Multiplier", colourProvider.Content2, ModUtils.FormatScoreMultiplier(multiplier)),
                            new StatisticRow(BeatmapsetsStrings.ShowScoreboardHeadersCombo, colourProvider.Content2, value.MaxCombo.ToLocalisableString(@"0\x")),
                            new StatisticRow(BeatmapsetsStrings.ShowScoreboardHeadersAccuracy, colourProvider.Content2, value.Accuracy.FormatAccuracy()),
                        };

                        if (value.PP != null)
                        {
                            generalStatistics = new[]
                            {
                                new StatisticRow(BeatmapsetsStrings.ShowScoreboardHeaderspp.ToUpper(), colourProvider.Content2, value.PP.ToLocalisableString("N0"))
                            }.Concat(generalStatistics).ToArray();
                        }

                        statistics.ChildrenEnumerable = judgementsStatistics
                                                        .Append(Empty().With(d => d.Height = 20))
                                                        .Concat(generalStatistics);
                    }
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    CornerRadius = corner_radius;
                    Masking = true;

                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4f,
                    };

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background4,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 4f),
                            Margin = new MarginPadding { Top = 8f },
                            Children = new Drawable[]
                            {
                                absoluteDate = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                    UseFullGlyphHeight = false,
                                },
                                relativeDate = new DrawableDate(default, OsuFont.Style.Caption1.Size)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Colour = colourProvider.Content2,
                                    UseFullGlyphHeight = false,
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    CornerRadius = corner_radius,
                                    Masking = true,
                                    Margin = new MarginPadding { Top = 4f },
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background3,
                                        },
                                        statistics = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Spacing = new Vector2(0f, 4f),
                                            Padding = new MarginPadding(8f),
                                        },
                                    },
                                },
                            },
                        },
                    };
                }
            }

            private partial class StatisticRow : CompositeDrawable
            {
                public StatisticRow(LocalisableString label, Color4 labelColour, LocalisableString value)
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChildren = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = label,
                            Colour = labelColour,
                            Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Text = value,
                            Colour = Color4.White,
                            Font = OsuFont.Style.Caption2,
                        },
                    };
                }
            }

            private partial class ModsPanel : CompositeDrawable
            {
                private FillFlowContainer modsFlow = null!;

                public ScoreInfo Score
                {
                    set
                    {
                        var mods = value.Mods;

                        if (!mods.Any())
                            Hide();
                        else
                        {
                            Show();

                            modsFlow.ChildrenEnumerable = mods.AsOrdered().Select(m => new ModIcon(m)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Scale = new Vector2(0.3f),
                            });
                        }
                    }
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    CornerRadius = corner_radius;
                    Masking = true;

                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4f,
                    };

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background4,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Transparent,
                        },
                        modsFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 6f, Top = 6f + spacing },
                            Padding = new MarginPadding { Horizontal = 16f },
                            Spacing = new Vector2(2f, -4f),
                        },
                    };
                }
            }

            public partial class TotalScoreRankPanel : CompositeDrawable
            {
                private Box rankBackground = null!;
                private Container<DrawableRank> rankContainer = null!;
                private OsuSpriteText totalScore = null!;

                [Resolved]
                private ScoreManager scoreManager { get; set; } = null!;

                public ScoreInfo Score
                {
                    set
                    {
                        rankBackground.Colour = ColourInfo.GradientVertical(
                            OsuColour.ForRank(value.Rank).Opacity(0f),
                            OsuColour.ForRank(value.Rank).Opacity(0.5f));
                        rankContainer.Child = new DrawableRank(value.Rank);
                        totalScore.Current = scoreManager.GetBindableTotalScoreString(value);
                    }
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    CornerRadius = corner_radius;
                    Masking = true;

                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4f,
                    };

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#353535"),
                        },
                        rankBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        rankContainer = new Container<DrawableRank>
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Size = new Vector2(25f, 14f),
                            Margin = new MarginPadding { Bottom = 5f },
                        },
                        totalScore = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding { Bottom = 25f, Top = 10f + spacing },
                            Font = OsuFont.Style.Subtitle.With(weight: FontWeight.Light, fixedWidth: true),
                            Spacing = new Vector2(-1.5f),
                            UseFullGlyphHeight = false,
                        },
                    };
                }
            }
        }
    }
}
