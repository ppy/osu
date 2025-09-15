// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
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

                private readonly Bindable<bool> prefer24HourTime = new Bindable<bool>();

                [Resolved]
                private OsuColour colours { get; set; } = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                private ScoreInfo score = null!;

                public ScoreInfo Score
                {
                    get => score;
                    set
                    {
                        score = value;

                        updateAbsoluteDate();
                        relativeDate.Date = value.Date;

                        var judgementsStatistics = value.GetStatisticsForDisplay().Select(s =>
                            new StatisticRow(s.DisplayName.ToUpper(), s.Count.ToLocalisableString("N0"), colours.ForHitResult(s.Result)));

                        double multiplier = 1.0;

                        foreach (var mod in value.Mods)
                            multiplier *= mod.ScoreMultiplier;

                        var generalStatistics = new[]
                        {
                            new StatisticRow(BeatmapsetsStrings.ShowScoreboardHeadersCombo, value.MaxCombo.ToLocalisableString(@"0\x")),
                            new StatisticRow(BeatmapsetsStrings.ShowScoreboardHeadersAccuracy, value.Accuracy.FormatAccuracy()),
                            new PerformanceStatisticRow(BeatmapsetsStrings.ShowScoreboardHeaderspp.ToUpper(), score),
                            Empty().With(d => d.Height = 20),
                            new StatisticRow(ModSelectOverlayStrings.ScoreMultiplier, ModUtils.FormatScoreMultiplier(multiplier)),
                        };

                        statistics.ChildrenEnumerable = judgementsStatistics
                                                        .Append(Empty().With(d => d.Height = 20))
                                                        .Concat(generalStatistics);
                    }
                }

                [BackgroundDependencyLoader]
                private void load(OsuConfigManager configManager)
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
                                            Spacing = new Vector2(0f, 2f),
                                            Padding = new MarginPadding(8f),
                                        },
                                    },
                                },
                            },
                        },
                    };

                    configManager.BindWith(OsuSetting.Prefer24HourTime, prefer24HourTime);
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    prefer24HourTime.BindValueChanged(_ => updateAbsoluteDate(), true);
                }

                private void updateAbsoluteDate()
                    => absoluteDate.Text = score.Date.ToLocalTime().ToLocalisableString(prefer24HourTime.Value ? @"d MMMM yyyy HH:mm" : @"d MMMM yyyy h:mm tt");
            }

            public partial class StatisticRow : CompositeDrawable
            {
                private readonly OsuSpriteText labelText;
                protected readonly OsuSpriteText ValueText;

                private readonly Color4? colour;

                public StatisticRow(LocalisableString label, LocalisableString value, Color4? colour = null)
                {
                    this.colour = colour;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChildren = new[]
                    {
                        labelText = new OsuSpriteText
                        {
                            Text = label,
                            Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                        },
                        ValueText = new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Text = value,
                            Colour = Color4.White,
                            Font = OsuFont.Style.Caption2,
                        },
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    labelText.Colour = colour ?? colourProvider.Content2;
                    ValueText.Colour = Interpolation.ValueAt(0.85f, colourProvider.Content1, colour ?? colourProvider.Content1, 0, 1);
                }
            }

            public partial class PerformanceStatisticRow : StatisticRow
            {
                private readonly ScoreInfo score;

                public PerformanceStatisticRow(LocalisableString label, ScoreInfo score)
                    : base(label, @"0pp")
                {
                    this.score = score;
                }

                [BackgroundDependencyLoader]
                private void load(BeatmapDifficultyCache difficultyCache, CancellationToken? cancellationToken)
                {
                    if (score.PP.HasValue)
                    {
                        setPerformanceValue(score, score.PP.Value);
                        return;
                    }

                    Task.Run(async () =>
                    {
                        var attributes = await difficultyCache.GetDifficultyAsync(score.BeatmapInfo!, score.Ruleset, score.Mods, cancellationToken ?? default).ConfigureAwait(false);
                        var performanceCalculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator();

                        // Performance calculation requires the beatmap and ruleset to be locally available. If not, return a default value.
                        if (attributes?.DifficultyAttributes == null || performanceCalculator == null)
                            return;

                        var result = await performanceCalculator.CalculateAsync(score, attributes.Value.DifficultyAttributes, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

                        Schedule(() => setPerformanceValue(score, result.Total));
                    }, cancellationToken ?? default);
                }

                private void setPerformanceValue(ScoreInfo scoreInfo, double pp)
                {
                    int ppValue = (int)Math.Round(pp, MidpointRounding.AwayFromZero);
                    ValueText.Text = LocalisableString.Interpolate(@$"{ppValue:N0}pp");

                    if (!scoreInfo.BeatmapInfo!.Status.GrantsPerformancePoints() || hasUnrankedMods(scoreInfo))
                        Alpha = 0.5f;
                    else
                        Alpha = 1f;
                }

                private static bool hasUnrankedMods(ScoreInfo scoreInfo)
                {
                    IEnumerable<Mod> modsToCheck = scoreInfo.Mods;

                    if (scoreInfo.IsLegacyScore)
                        modsToCheck = modsToCheck.Where(m => m is not ModClassic);

                    return modsToCheck.Any(m => !m.Ranked);
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
