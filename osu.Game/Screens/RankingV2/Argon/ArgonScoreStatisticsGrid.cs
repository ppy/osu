// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class ArgonScoreStatisticsGrid : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        private const double cell_fade_in_time = 300;
        private const double cell_fade_in_overlap = 120;

        private StatisticsCell accuracyCell = null!;
        private StatisticsCell comboCell = null!;
        private StatisticsCell ppCell = null!;
        private GridContainer basicStatsFirstRow = null!;
        private GridContainer basicStatsSecondRow = null!;
        private GridContainer extendedStatsRow = null!;
        private ModCell modCell = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;
            Width = 600;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Shear = OsuGame.SHEAR,
                Direction = FillDirection.Vertical,
                Children =
                [
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                accuracyCell = new StatisticsCell
                                {
                                    Caption = BeatmapsetsStrings.ShowScoreboardHeadersAccuracy,
                                },
                                comboCell = new StatisticsCell
                                {
                                    Caption = BeatmapsetsStrings.ShowScoreboardHeadersCombo,
                                },
                                ppCell = new StatisticsCell
                                {
                                    Caption = ResultsScreenStrings.PerformancePointsHeader,
                                }
                            },
                        },
                    },
                    basicStatsFirstRow = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    },
                    basicStatsSecondRow = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    },
                    extendedStatsRow = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = 10 },
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    },
                    modCell = new ModCell
                    {
                        Margin = new MarginPadding { Top = 10 },
                    }
                ]
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateScore(), true);
        }

        protected override void Update()
        {
            base.Update();

            Padding = new MarginPadding { Right = DrawHeight * OsuGame.SHEAR.X };
        }

        private void updateScore()
        {
            var newScore = score.Value;

            var ruleset = newScore.Ruleset.CreateInstance();
            var mods = newScore.Mods.Select(m => m.ToMod(ruleset)).ToArray();

            accuracyCell.Value = newScore.Accuracy.FormatAccuracy();
            accuracyCell.AccentColour = newScore.Accuracy == 1 ? ArgonTotalScorePanel.PERFECT_GRADIENT : null;

            comboCell.Value = LocalisableString.Interpolate($"{newScore.MaxCombo}x");
            comboCell.AccentColour = newScore.IsPerfect() ? ArgonTotalScorePanel.PERFECT_GRADIENT : null;

            // TODO: local scores do not have PP actually populated in the database, it will need to be recalculated on the spot
            setPerformanceValue(newScore, mods);

            var hitStatistics = newScore.GetStatisticsForDisplay().ToArray();
            var basicHitStatistics = hitStatistics.Where(s => s.Result.IsBasic()).ToArray();
            var otherHitStatistics = hitStatistics.Where(s => !s.Result.IsBasic()).ToArray();

            basicStatsFirstRow.Content = new[]
            {
                basicHitStatistics.Take(basicHitStatistics.Length > 4 ? basicHitStatistics.Length / 2 : 4).Select(s => new StatisticsCell
                {
                    Caption = s.DisplayName,
                    Value = s.Count.ToLocalisableString(@"N0"),
                    BaseFontSize = 32,
                    AccentColour = colours.ForHitResult(s.Result),
                }).ToArray<Drawable>(),
            };
            basicStatsSecondRow.Content = new[]
            {
                basicHitStatistics.Skip(basicHitStatistics.Length > 4 ? basicHitStatistics.Length / 2 : 4).Where(s => s.Result <= HitResult.Perfect).Select(s => new StatisticsCell
                {
                    Caption = s.DisplayName,
                    Value = s.Count.ToLocalisableString(@"N0"),
                    BaseFontSize = 32,
                    AccentColour = colours.ForHitResult(s.Result),
                }).ToArray<Drawable>(),
            };
            extendedStatsRow.Content = new[]
            {
                otherHitStatistics.Select(s => new StatisticsCell
                {
                    Caption = s.DisplayName,
                    Value = s.Count.ToLocalisableString(@"N0"),
                    MaxValue = s.MaxCount?.ToLocalisableString(@"N0"),
                    BaseFontSize = 28,
                }).ToArray<Drawable>(),
            };

            modCell.Alpha = mods.Length > 0 ? 1 : 0;
            modCell.ModDisplay.Current.Value = mods;
        }

        private void setPerformanceValue(IScoreInfo newScore, IEnumerable<Mod> mods)
        {
            if (newScore.PP.HasValue)
            {
                ppCell.Value = LocalisableString.Interpolate($@"{(int)Math.Round(newScore.PP.Value, MidpointRounding.AwayFromZero):N0}pp");

                if (newScore.Beatmap?.Status.GrantsPerformancePoints() != true)
                {
                    ppCell.Alpha = 0.5f;
                    ppCell.TooltipText = ResultsScreenStrings.NoPPForUnrankedBeatmaps;
                }
                else if (hasUnrankedMods(mods, newScore.IsLegacyScore))
                {
                    ppCell.Alpha = 0.5f;
                    ppCell.TooltipText = ResultsScreenStrings.NoPPForUnrankedMods;
                }
                else if (newScore.Rank == ScoreRank.F)
                {
                    ppCell.Alpha = 0.5f;
                    ppCell.TooltipText = ResultsScreenStrings.NoPPForFailedScores;
                }
                else
                {
                    ppCell.Alpha = 1f;
                    ppCell.TooltipText = default;
                }
            }
            else
            {
                ppCell.Value = "-";
                ppCell.Alpha = 0.5f;
            }
        }

        private static bool hasUnrankedMods(IEnumerable<Mod> modsToCheck, bool isLegacyScore)
        {
            if (isLegacyScore)
                modsToCheck = modsToCheck.Where(m => m is not ModClassic);

            return modsToCheck.Any(m => !m.Ranked);
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the argon skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt(3)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            FinishAnimating();

            double lastTransformStartTime = startTime + cell_fade_in_overlap;

            foreach (var cell in this.ChildrenOfType<StatisticsCell>())
                lastTransformStartTime = cell.StartAnimating(lastTransformStartTime) - cell_fade_in_overlap;

            if (score.Value.Mods.Any())
                lastTransformStartTime = modCell.StartAnimating(lastTransformStartTime);

            return lastTransformStartTime;
        }

        public void FinishAnimating()
        {
            foreach (var cell in this.ChildrenOfType<StatisticsCell>())
                cell.FinishAnimating();

            modCell.FinishAnimating();
        }

        #endregion

        public partial class StatisticsCell : CompositeDrawable, IHasTooltip
        {
            public required LocalisableString Caption { get; init; }

            private LocalisableString value;

            public LocalisableString Value
            {
                get => value;
                set
                {
                    this.value = value;

                    if (IsLoaded)
                        updateState();
                }
            }

            private LocalisableString? maxValue;

            public LocalisableString? MaxValue
            {
                get => value;
                set
                {
                    maxValue = value;

                    if (IsLoaded)
                        updateState();
                }
            }

            public float BaseFontSize { get; init; } = 36;

            private ColourInfo? accentColour { get; set; }

            public ColourInfo? AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;

                    if (IsLoaded)
                        updateState();
                }
            }

            private Container contentContainer = null!;
            private Box backgroundBox = null!;
            private OsuSpriteText captionText = null!;
            private OsuSpriteText valueText = null!;
            private OsuSpriteText maxValueText = null!;

            private const float padding = 5;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                // constant height ensures correctness of transition playback
                // (no jumps from sudden autosize adjustments once components become present)
                Height = BaseFontSize + (BaseFontSize - 12) + 2 * padding;
                Shear = -OsuGame.SHEAR;
                Padding = new MarginPadding { Right = padding, };
                Margin = new MarginPadding { Bottom = padding, };

                InternalChild = contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect(),
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Children =
                    [
                        backgroundBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 7,
                            },
                            Spacing = new Vector2(0, -3),
                            Children =
                            [
                                captionText = new OsuSpriteText
                                {
                                    Text = Caption.ToUpper(),
                                    Font = OsuFont.Default.With(size: BaseFontSize - 12, weight: FontWeight.Bold),
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(3),
                                    Children =
                                    [
                                        valueText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.Default.With(size: BaseFontSize),
                                        },
                                        maxValueText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.Default.With(size: BaseFontSize - 10),
                                            Margin = new MarginPadding { Bottom = 2 },
                                        },
                                    ],
                                },
                            ],
                        }
                    ]
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            private void updateState()
            {
                valueText.Text = value;
                maxValueText.Text = maxValue == null ? default : LocalisableString.Interpolate($"/ {maxValue}");

                if (AccentColour != null)
                {
                    backgroundBox.Colour = Interpolation.ValueAt(0.05, (ColourInfo)colourProvider.Background5, AccentColour.Value, 0, 1);
                    contentContainer.BorderThickness = 2;
                    contentContainer.BorderColour = AccentColour.Value;
                    captionText.Colour = valueText.Colour = maxValueText.Colour = ColourInfo.GradientVertical(AccentColour.Value.TopLeft, AccentColour.Value.BottomRight);
                }
                else
                {
                    backgroundBox.Colour = colourProvider.Background5;
                    contentContainer.BorderThickness = 0;
                    captionText.Colour = colourProvider.Content1;
                    valueText.Colour = maxValueText.Colour = ArgonTotalScorePanel.TEXT_GRADIENT;
                }
            }

            public double StartAnimating(double startTime)
            {
                contentContainer.FadeOut()
                                .MoveToOffset(new Vector2(-30, 0));

                using (BeginAbsoluteSequence(startTime))
                {
                    contentContainer.FadeIn(cell_fade_in_time, Easing.OutQuint)
                                    .MoveToOffset(new Vector2(30, 0), cell_fade_in_time, Easing.OutQuint);

                    return contentContainer.LatestTransformEndTime;
                }
            }

            public void FinishAnimating()
            {
                contentContainer.FinishTransforms();
            }

            public LocalisableString TooltipText { get; set; }
        }

        public partial class ModCell : CompositeDrawable
        {
            public ModDisplay ModDisplay { get; private set; } = null!;

            private Container contentContainer = null!;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                // constant height ensures correctness of transition playback
                // (no jumps from sudden autosize adjustments once components become present)
                Height = 70;
                Shear = -OsuGame.SHEAR;
                Padding = new MarginPadding { Right = 5, };

                InternalChild = contentContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect(),
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding
                            {
                                Horizontal = 7,
                                Vertical = 5,
                            },
                            Children =
                            [
                                new OsuSpriteText
                                {
                                    Text = @"Mods".ToUpperInvariant(),
                                    Font = OsuFont.Default.With(size: 16, weight: FontWeight.Bold),
                                },
                                ModDisplay = new ModDisplay
                                {
                                    Scale = new Vector2(0.8f),
                                    ShowExtendedInformation = true,
                                    ExpansionMode = ExpansionMode.AlwaysExpanded
                                }
                            ],
                        }
                    ]
                };
            }

            public double StartAnimating(double startTime)
            {
                contentContainer.FadeOut()
                                .MoveToOffset(new Vector2(-30, 0));

                using (BeginAbsoluteSequence(startTime))
                {
                    contentContainer.FadeIn(cell_fade_in_time, Easing.OutQuint)
                                    .MoveToOffset(new Vector2(30, 0), cell_fade_in_time, Easing.OutQuint);

                    return contentContainer.LatestTransformEndTime;
                }
            }

            public void FinishAnimating()
            {
                contentContainer.FinishTransforms();
            }
        }
    }
}
