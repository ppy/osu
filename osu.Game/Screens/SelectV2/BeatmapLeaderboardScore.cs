// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using CommonStrings = osu.Game.Localisation.CommonStrings;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapLeaderboardScore : OsuClickableContainer, IHasContextMenu, IHasCustomTooltip<ScoreInfo>
    {
        public Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>();

        /// <summary>
        /// A function determining whether each mod in the score can be selected.
        /// A return value of <see langword="true"/> means that the mod can be selected in the current context.
        /// A return value of <see langword="false"/> means that the mod cannot be selected in the current context.
        /// </summary>
        public Func<Mod, bool> IsValidMod { get; set; } = _ => true;

        public int? Rank { get; init; }
        public bool IsPersonalBest { get; init; }

        private const float expanded_right_content_width = 200;
        private const float grade_width = 35;
        private const float username_min_width = 120;
        private const float statistics_regular_min_width = 165;
        private const float statistics_compact_min_width = 90;
        private const float rank_label_width = 60;

        private readonly ScoreInfo score;
        private readonly bool sheared;

        public const int HEIGHT = 50;

        private const int corner_radius = 10;
        private const int transition_duration = 200;

        private Colour4 foregroundColour;
        private Colour4 backgroundColour;
        private ColourInfo totalScoreBackgroundGradient;

        private static readonly Color4 personal_best_gradient_left = Color4Extensions.FromHex("#66FFCC");
        private static readonly Color4 personal_best_gradient_right = Color4Extensions.FromHex("#51A388");
        private ColourInfo personalBestGradient;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private Clipboard? clipboard { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Container content = null!;
        private Box background = null!;
        private Box foreground = null!;

        private Drawable avatar = null!;
        private ClickableAvatar innerAvatar = null!;

        private OsuSpriteText nameLabel = null!;
        private List<ScoreComponentLabel> statisticsLabels = null!;

        private Container rightContent = null!;

        protected Container RankContainer { get; private set; } = null!;
        private FillFlowContainer flagBadgeAndDateContainer = null!;
        private FillFlowContainer modsContainer = null!;

        private OsuSpriteText scoreText = null!;
        private Drawable scoreRank = null!;
        private Box totalScoreBackground = null!;

        private FillFlowContainer statisticsContainer = null!;
        private Container personalBestIndicator = null!;
        private Container rankLabelStandalone = null!;
        private Container rankLabelOverlay = null!;

        public ITooltip<ScoreInfo> GetCustomTooltip() => new LeaderboardScoreTooltip(colourProvider);
        public virtual ScoreInfo TooltipContent => score;

        public BeatmapLeaderboardScore(ScoreInfo score, bool sheared = true)
        {
            this.score = score;
            this.sheared = sheared;

            Shear = sheared ? OsuGame.SHEAR : Vector2.Zero;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var user = score.User;

            foregroundColour = colourProvider.Background5;
            backgroundColour = colourProvider.Background3;
            totalScoreBackgroundGradient = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), backgroundColour);
            personalBestGradient = ColourInfo.GradientHorizontal(personal_best_gradient_left, personal_best_gradient_right);

            statisticsLabels = GetStatistics(score).Select(s => new ScoreComponentLabel(s, score)
            {
                // ensure statistics container is the correct width when invalidating
                AlwaysPresent = true,
            }).ToList();

            Child = content = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                rankLabelStandalone = new Container
                                {
                                    Width = rank_label_width,
                                    RelativeSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        personalBestIndicator = new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Right = -10f },
                                            Alpha = IsPersonalBest ? 1 : 0,
                                            Colour = personalBestGradient,
                                            Child = new Box { RelativeSizeAxes = Axes.Both },
                                        },
                                        new RankLabel(Rank, sheared, darkText: IsPersonalBest)
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        }
                                    },
                                },
                                createCentreContent(user),
                                createRightContent()
                            }
                        }
                    }
                }
            };

            innerAvatar.OnLoadComplete += d => d.FadeInFromZero(200);
        }

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private IBindable<ScoringMode> scoringMode { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
            scoringMode.BindValueChanged(s =>
            {
                switch (s.NewValue)
                {
                    case ScoringMode.Standardised:
                        rightContent.Width = 170;
                        break;

                    case ScoringMode.Classic:
                        rightContent.Width = expanded_right_content_width;
                        break;
                }

                updateModDisplay();
            }, true);
        }

        private void updateModDisplay()
        {
            int maxMods = scoringMode.Value == ScoringMode.Standardised ? 4 : 5;

            if (score.Mods.Length > 0)
            {
                modsContainer.Padding = new MarginPadding { Top = 4f };
                modsContainer.ChildrenEnumerable = score.Mods.AsOrdered().Take(Math.Min(maxMods, score.Mods.Length)).Select(mod => new ColouredModSwitchTiny(mod)
                {
                    Scale = new Vector2(0.3125f)
                });

                if (score.Mods.Length > maxMods)
                {
                    modsContainer.Remove(modsContainer[^1], true);
                    modsContainer.Add(new MoreModSwitchTiny(score.Mods)
                    {
                        Scale = new Vector2(0.3125f),
                    });
                }
            }
        }

        private Container createCentreContent(APIUser user) => new Container
        {
            Name = @"Centre container",
            Masking = true,
            CornerRadius = corner_radius,
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                foreground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = foregroundColour
                },
                new UserCoverBackground
                {
                    RelativeSizeAxes = Axes.Both,
                    User = score.User,
                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Colour = ColourInfo.GradientHorizontal(Colour4.White.Opacity(0.5f), Colour4.FromHex(@"222A27").Opacity(1)),
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                CornerRadius = corner_radius,
                                Masking = true,
                                Children = new[]
                                {
                                    avatar = new DelayedLoadWrapper(
                                        innerAvatar = new ClickableAvatar(user)
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Scale = new Vector2(1.1f),
                                            Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                            RelativeSizeAxes = Axes.Both,
                                        })
                                    {
                                        RelativeSizeAxes = Axes.None,
                                        Size = new Vector2(HEIGHT)
                                    },
                                    rankLabelOverlay = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = Colour4.Black.Opacity(0.5f),
                                            },
                                            new RankLabel(Rank, sheared, false)
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                            },
                                        }
                                    }
                                },
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding { Horizontal = corner_radius },
                                Children = new Drawable[]
                                {
                                    flagBadgeAndDateContainer = new FillFlowContainer
                                    {
                                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(5),
                                        AutoSizeAxes = Axes.Both,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new UpdateableFlag(user.CountryCode)
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Size = new Vector2(20, 14),
                                            },
                                            new UpdateableTeamFlag(user.Team)
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Size = new Vector2(30, 15),
                                            },
                                            new DateLabel(score.Date)
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Colour = colourProvider.Content2,
                                                UseFullGlyphHeight = false,
                                            }
                                        }
                                    },
                                    nameLabel = new TruncatingSpriteText
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                        Text = user.Username,
                                        Font = OsuFont.Style.Heading2,
                                    }
                                }
                            },
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Child = statisticsContainer = new FillFlowContainer
                                {
                                    Name = @"Statistics container",
                                    Padding = new MarginPadding { Right = 40 },
                                    Spacing = new Vector2(25, 0),
                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = statisticsLabels,
                                    Alpha = 0,
                                    LayoutEasing = Easing.OutQuint,
                                    LayoutDuration = transition_duration,
                                }
                            }
                        }
                    },
                },
            },
        };

        private Container createRightContent() => rightContent = new Container
        {
            Name = @"Right content",
            RelativeSizeAxes = Axes.Y,
            Child = new Container
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = grade_width },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), OsuColour.ForRank(score.Rank)),
                        },
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = grade_width,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Colour = OsuColour.ForRank(score.Rank),
                    },
                    new TrianglesV2
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        SpawnRatio = 2,
                        Velocity = 0.7f,
                        Colour = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), OsuColour.ForRank(score.Rank).Darken(0.2f)),
                    },
                    RankContainer = new Container
                    {
                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = grade_width,
                        Child = scoreRank = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(-2),
                            Colour = DrawableRank.GetRankNameColour(score.Rank),
                            Font = OsuFont.Numeric.With(size: 14),
                            Text = DrawableRank.GetRankName(score.Rank),
                            ShadowColour = Color4.Black.Opacity(0.3f),
                            ShadowOffset = new Vector2(0, 0.08f),
                            Shadow = true,
                            UseFullGlyphHeight = false,
                        },
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Padding = new MarginPadding { Right = grade_width },
                        Child = new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Masking = true,
                            CornerRadius = corner_radius,
                            Children = new Drawable[]
                            {
                                totalScoreBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = totalScoreBackgroundGradient,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), OsuColour.ForRank(score.Rank).Opacity(0.5f)),
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Horizontal = corner_radius },
                                    Children = new Drawable[]
                                    {
                                        scoreText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            UseFullGlyphHeight = false,
                                            Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                            Current = scoreManager.GetBindableTotalScoreString(score),
                                            Spacing = new Vector2(-1.5f),
                                            Font = OsuFont.Style.Subtitle.With(weight: FontWeight.Light, fixedWidth: true),
                                        },
                                        new InputBlockingContainer
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            AutoSizeAxes = Axes.Both,
                                            Child = modsContainer = new FillFlowContainer
                                            {
                                                Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(2f, 0f),
                                            },
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            },
        };

        protected (CaseTransformableString, LocalisableString DisplayAccuracy)[] GetStatistics(ScoreInfo model) => new[]
        {
            (BeatmapsetsStrings.ShowScoreboardHeadersCombo.ToUpper(), model.MaxCombo.ToString().Insert(model.MaxCombo.ToString().Length, "x")),
            (BeatmapsetsStrings.ShowScoreboardHeadersAccuracy.ToUpper(), model.DisplayAccuracy),
        };

        public override void Show()
        {
            foreach (var d in new[] { avatar, nameLabel, scoreText, scoreRank, flagBadgeAndDateContainer, modsContainer }.Concat(statisticsLabels))
                d.FadeOut();

            Alpha = 0;

            content.MoveToY(60);
            avatar.MoveToX(60);
            nameLabel.MoveToX(125);

            this.FadeIn(200);
            content.MoveToY(0, 800, Easing.OutQuint);

            using (BeginDelayedSequence(100))
            {
                avatar.FadeIn(300, Easing.OutQuint);
                nameLabel.FadeIn(350, Easing.OutQuint);

                avatar.MoveToX(0, 300, Easing.OutQuint);
                nameLabel.MoveToX(0, 350, Easing.OutQuint);

                using (BeginDelayedSequence(250))
                {
                    scoreText.FadeIn(200);
                    scoreRank.FadeIn(200);

                    using (BeginDelayedSequence(50))
                    {
                        var drawables = new Drawable[] { flagBadgeAndDateContainer, modsContainer }.Concat(statisticsLabels).ToArray();
                        for (int i = 0; i < drawables.Length; i++)
                            drawables[i].FadeIn(100 + i * 50);
                    }
                }
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            var lightenedGradient = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0).Lighten(0.2f), backgroundColour.Lighten(0.2f));
            var personalBestLightenedGradient = ColourInfo.GradientHorizontal(personal_best_gradient_left.Lighten(0.2f), personal_best_gradient_right.Lighten(0.2f));

            foreground.FadeColour(IsHovered ? foregroundColour.Lighten(0.2f) : foregroundColour, transition_duration, Easing.OutQuint);
            background.FadeColour(IsHovered ? backgroundColour.Lighten(0.2f) : backgroundColour, transition_duration, Easing.OutQuint);
            totalScoreBackground.FadeColour(IsHovered ? lightenedGradient : totalScoreBackgroundGradient, transition_duration, Easing.OutQuint);
            personalBestIndicator.FadeColour(IsHovered ? personalBestLightenedGradient : personalBestGradient, transition_duration, Easing.OutQuint);

            if (IsHovered && currentMode != DisplayMode.Full)
                rankLabelOverlay.FadeIn(transition_duration, Easing.OutQuint);
            else
                rankLabelOverlay.FadeOut(transition_duration, Easing.OutQuint);
        }

        private DisplayMode? currentMode;

        protected override void Update()
        {
            base.Update();

            DisplayMode mode = getCurrentDisplayMode();

            if (currentMode != mode)
            {
                if (mode >= DisplayMode.Full)
                    rankLabelStandalone.FadeIn(transition_duration, Easing.OutQuint).ResizeWidthTo(rank_label_width, transition_duration, Easing.OutQuint);
                else
                    rankLabelStandalone.FadeOut(transition_duration, Easing.OutQuint).ResizeWidthTo(0, transition_duration, Easing.OutQuint);

                if (mode >= DisplayMode.Regular)
                {
                    statisticsContainer.FadeIn(transition_duration, Easing.OutQuint).MoveToX(0, transition_duration, Easing.OutQuint);
                    statisticsContainer.Direction = FillDirection.Horizontal;
                    statisticsContainer.ScaleTo(1, transition_duration, Easing.OutQuint);
                }
                else if (mode >= DisplayMode.Compact)
                {
                    statisticsContainer.FadeIn(transition_duration, Easing.OutQuint).MoveToX(0, transition_duration, Easing.OutQuint);
                    statisticsContainer.Direction = FillDirection.Vertical;
                    statisticsContainer.ScaleTo(0.8f, transition_duration, Easing.OutQuint);
                }
                else
                    statisticsContainer.FadeOut(transition_duration, Easing.OutQuint).MoveToX(statisticsContainer.DrawWidth, transition_duration, Easing.OutQuint);

                currentMode = mode;
            }
        }

        private DisplayMode getCurrentDisplayMode()
        {
            if (DrawWidth >= HEIGHT + username_min_width + statistics_regular_min_width + expanded_right_content_width + rank_label_width)
                return DisplayMode.Full;

            if (DrawWidth >= HEIGHT + username_min_width + statistics_regular_min_width + expanded_right_content_width)
                return DisplayMode.Regular;

            if (DrawWidth >= HEIGHT + username_min_width + statistics_compact_min_width + expanded_right_content_width)
                return DisplayMode.Compact;

            return DisplayMode.Minimal;
        }

        #region Subclasses

        private enum DisplayMode
        {
            Minimal,
            Compact,
            Regular,
            Full
        }

        private partial class DateLabel : DrawableDate
        {
            public DateLabel(DateTimeOffset date)
                : base(date)
            {
                Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold);
            }

            protected override string Format() => Date.ToShortRelativeTime(TimeSpan.FromSeconds(30));
        }

        private partial class ScoreComponentLabel : Container
        {
            private readonly (LocalisableString Name, LocalisableString Value) statisticInfo;
            private readonly ScoreInfo score;

            private FillFlowContainer content = null!;
            public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

            public ScoreComponentLabel((LocalisableString Name, LocalisableString Value) statisticInfo, ScoreInfo score)
            {
                this.statisticInfo = statisticInfo;
                this.score = score;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                OsuSpriteText value;
                Child = content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Colour = colourProvider.Content2,
                            Text = statisticInfo.Name,
                            Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                        },
                        value = new OsuSpriteText
                        {
                            // We don't want the value setting the horizontal size, since it leads to wonky accuracy container length,
                            // since the accuracy is sometimes longer than its name.
                            BypassAutoSizeAxes = Axes.X,
                            Text = statisticInfo.Value,
                            Font = OsuFont.Style.Body,
                        }
                    }
                };

                if (score.Combo != score.MaxCombo && statisticInfo.Name == BeatmapsetsStrings.ShowScoreboardHeadersCombo)
                    value.Colour = colours.Lime1;
            }
        }

        private partial class RankLabel : Container, IHasTooltip
        {
            private readonly bool darkText;
            private readonly OsuSpriteText text;

            public RankLabel(int? rank, bool sheared, bool darkText)
            {
                this.darkText = darkText;
                if (rank >= 1000)
                    TooltipText = $"#{rank:N0}";

                Child = text = new OsuSpriteText
                {
                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Style.Heading2,
                    Text = rank == null ? "-" : rank.Value.FormatRank().Insert(0, "#"),
                    Shadow = !darkText,
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                text.Colour = darkText ? colourProvider.Background3 : colourProvider.Content1;
            }

            public LocalisableString TooltipText { get; }
        }

        private sealed partial class ColouredModSwitchTiny : ModSwitchTiny, IHasCustomTooltip<Mod>
        {
            public Mod? TooltipContent { get; }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ColouredModSwitchTiny(Mod mod)
                : base(mod)
            {
                TooltipContent = mod;
                Active.Value = true;
            }

            public ITooltip<Mod> GetCustomTooltip() => new ModTooltip(colourProvider);
        }

        private sealed partial class MoreModSwitchTiny : CompositeDrawable, IHasPopover
        {
            private readonly IReadOnlyList<Mod> mods;

            public MoreModSwitchTiny(IReadOnlyList<Mod> mods)
            {
                this.mods = mods;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Size = new Vector2(ModSwitchTiny.WIDTH, ModSwitchTiny.DEFAULT_HEIGHT);

                InternalChild = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background6,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = false,
                            Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold),
                            Text = ". . .",
                            Colour = Color4.White,
                            UseFullGlyphHeight = false,
                            Margin = new MarginPadding
                            {
                                Top = 4
                            }
                        }
                    }
                };
            }

            protected override bool OnClick(ClickEvent e)
            {
                this.ShowPopover();
                return true;
            }

            protected override bool OnHover(HoverEvent e) => true;

            public Popover GetPopover() => new MoreModsPopover(mods);
        }

        public partial class MoreModsPopover : OsuPopover
        {
            public MoreModsPopover(IReadOnlyList<Mod> mods)
            {
                AutoSizeAxes = Axes.Both;
                AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

                Child = new FillFlowContainer
                {
                    Width = 125f,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Spacing = new Vector2(2.5f),
                    ChildrenEnumerable = mods.AsOrdered().Select(m => new ColouredModSwitchTiny(m)
                    {
                        Scale = new Vector2(0.3125f),
                    })
                };
            }
        }

        #endregion

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                // system mods should never be copied across regardless of anything.
                var copyableMods = score.Mods.Where(m => IsValidMod.Invoke(m) && m.Type != ModType.System).ToArray();

                if (copyableMods.Length > 0)
                    items.Add(new OsuMenuItem("Use these mods", MenuItemType.Highlighted, () => SelectedMods.Value = copyableMods));

                if (score.OnlineID > 0)
                    items.Add(new OsuMenuItem(CommonStrings.CopyLink, MenuItemType.Standard, () => clipboard?.SetText($@"{api.Endpoints.WebsiteUrl}/scores/{score.OnlineID}")));

                if (score.Files.Count <= 0) return items.ToArray();

                items.Add(new OsuMenuItem(CommonStrings.Export, MenuItemType.Standard, () => scoreManager.Export(score)));
                items.Add(new OsuMenuItem(Resources.Localisation.Web.CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new LocalScoreDeleteDialog(score))));

                return items.ToArray();
            }
        }
    }
}
