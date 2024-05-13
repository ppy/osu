// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardScoreV2 : OsuClickableContainer, IHasContextMenu, IHasCustomTooltip<ScoreInfo>
    {
        /// <summary>
        /// The maximum number of mods when contracted until the mods display width exceeds the <see cref="right_content_width"/>.
        /// </summary>
        public const int MAX_MODS_CONTRACTED = 13;

        /// <summary>
        /// The maximum number of mods when expanded until the mods display width exceeds the <see cref="right_content_width"/>.
        /// </summary>
        public const int MAX_MODS_EXPANDED = 4;

        private const float right_content_width = 180;
        private const float grade_width = 40;
        private const float username_min_width = 125;
        private const float statistics_regular_min_width = 175;
        private const float statistics_compact_min_width = 100;
        private const float rank_label_width = 65;
        private const float rank_label_visibility_width_cutoff = rank_label_width + height + username_min_width + statistics_regular_min_width + right_content_width;

        private readonly ScoreInfo score;

        private const int height = 60;
        private const int corner_radius = 10;
        private const int transition_duration = 200;

        private readonly int? rank;

        private readonly bool isPersonalBest;

        private Colour4 foregroundColour;
        private Colour4 backgroundColour;
        private ColourInfo totalScoreBackgroundGradient;

        private static readonly Vector2 shear = new Vector2(0.15f, 0);

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        private Container content = null!;
        private Box background = null!;
        private Box foreground = null!;

        private Drawable avatar = null!;
        private ClickableAvatar innerAvatar = null!;

        private OsuSpriteText nameLabel = null!;
        private List<ScoreComponentLabel> statisticsLabels = null!;

        protected Container RankContainer { get; private set; } = null!;
        private FillFlowContainer flagBadgeAndDateContainer = null!;
        private FillFlowContainer<ColouredModSwitchTiny> modsContainer = null!;
        private OsuSpriteText modsCounter = null!;

        private OsuSpriteText scoreText = null!;
        private Drawable scoreRank = null!;
        private Box totalScoreBackground = null!;

        private FillFlowContainer statisticsContainer = null!;
        private RankLabel rankLabel = null!;
        private Container rankLabelOverlay = null!;

        public ITooltip<ScoreInfo> GetCustomTooltip() => new LeaderboardScoreTooltip();
        public virtual ScoreInfo TooltipContent => score;

        public LeaderboardScoreV2(ScoreInfo score, int? rank, bool isPersonalBest = false)
        {
            this.score = score;
            this.rank = rank;
            this.isPersonalBest = isPersonalBest;

            Shear = shear;
            RelativeSizeAxes = Axes.X;
            Height = height;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var user = score.User;

            foregroundColour = isPersonalBest ? colourProvider.Background1 : colourProvider.Background5;
            backgroundColour = isPersonalBest ? colourProvider.Background2 : colourProvider.Background4;
            totalScoreBackgroundGradient = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), backgroundColour);

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
                            new Dimension(GridSizeMode.Absolute, right_content_width),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    AutoSizeAxes = Axes.X,
                                    RelativeSizeAxes = Axes.Y,
                                    Child = rankLabel = new RankLabel(rank)
                                    {
                                        Width = rank_label_width,
                                        RelativeSizeAxes = Axes.Y,
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

            modsContainer.Spacing = new Vector2(modsContainer.Children.Count > MAX_MODS_EXPANDED ? -20 : 2, 0);
            modsContainer.Padding = new MarginPadding { Top = modsContainer.Children.Count > 0 ? 4 : 0 };
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
                    Shear = -shear,
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
                                            Shear = -shear,
                                            RelativeSizeAxes = Axes.Both,
                                        })
                                    {
                                        RelativeSizeAxes = Axes.None,
                                        Size = new Vector2(height)
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
                                            new RankLabel(rank)
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
                                        Shear = -shear,
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
                                                Size = new Vector2(24, 16),
                                            },
                                            new DateLabel(score.Date)
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                UseFullGlyphHeight = false,
                                            }
                                        }
                                    },
                                    nameLabel = new TruncatingSpriteText
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Shear = -shear,
                                        Text = user.Username,
                                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold)
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
                                    Shear = -shear,
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

        private Container createRightContent() => new Container
        {
            Name = @"Right content",
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
                    Shear = -shear,
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
                        Font = OsuFont.Numeric.With(size: 16),
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
                                        Shear = -shear,
                                        Current = scoreManager.GetBindableTotalScoreString(score),
                                        Font = OsuFont.GetFont(size: 30, weight: FontWeight.Light),
                                    },
                                    modsContainer = new FillFlowContainer<ColouredModSwitchTiny>
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Shear = -shear,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        ChildrenEnumerable = score.Mods.Select(mod => new ColouredModSwitchTiny(mod) { Scale = new Vector2(0.375f) })
                                    },
                                    modsCounter = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Shear = -shear,
                                        Text = $"{score.Mods.Length} mods",
                                        Alpha = 0,
                                    }
                                }
                            }
                        }
                    }
                }
            }
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

            content.MoveToY(75);
            avatar.MoveToX(75);
            nameLabel.MoveToX(150);

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
                        Drawable modsDrawable = score.Mods.Length > MAX_MODS_CONTRACTED ? modsCounter : modsContainer;
                        var drawables = new[] { flagBadgeAndDateContainer, modsDrawable }.Concat(statisticsLabels).ToArray();
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

            foreground.FadeColour(IsHovered ? foregroundColour.Lighten(0.2f) : foregroundColour, transition_duration, Easing.OutQuint);
            background.FadeColour(IsHovered ? backgroundColour.Lighten(0.2f) : backgroundColour, transition_duration, Easing.OutQuint);
            totalScoreBackground.FadeColour(IsHovered ? lightenedGradient : totalScoreBackgroundGradient, transition_duration, Easing.OutQuint);

            if (DrawWidth < rank_label_visibility_width_cutoff && IsHovered)
                rankLabelOverlay.FadeIn(transition_duration, Easing.OutQuint);
            else
                rankLabelOverlay.FadeOut(transition_duration, Easing.OutQuint);
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            Scheduler.AddOnce(() =>
            {
                // when width decreases
                // - hide rank and show rank overlay on avatar when hovered, then
                // - compact statistics, then
                // - hide statistics

                if (DrawWidth >= rank_label_visibility_width_cutoff)
                    rankLabel.FadeIn(transition_duration, Easing.OutQuint).MoveToX(0, transition_duration, Easing.OutQuint);
                else
                    rankLabel.FadeOut(transition_duration, Easing.OutQuint).MoveToX(-rankLabel.DrawWidth, transition_duration, Easing.OutQuint);

                if (DrawWidth >= height + username_min_width + statistics_regular_min_width + right_content_width)
                {
                    statisticsContainer.FadeIn(transition_duration, Easing.OutQuint).MoveToX(0, transition_duration, Easing.OutQuint);
                    statisticsContainer.Direction = FillDirection.Horizontal;
                    statisticsContainer.ScaleTo(1, transition_duration, Easing.OutQuint);
                }
                else if (DrawWidth >= height + username_min_width + statistics_compact_min_width + right_content_width)
                {
                    statisticsContainer.FadeIn(transition_duration, Easing.OutQuint).MoveToX(0, transition_duration, Easing.OutQuint);
                    statisticsContainer.Direction = FillDirection.Vertical;
                    statisticsContainer.ScaleTo(0.8f, transition_duration, Easing.OutQuint);
                }
                else
                    statisticsContainer.FadeOut(transition_duration, Easing.OutQuint).MoveToX(statisticsContainer.DrawWidth, transition_duration, Easing.OutQuint);
            });

            return base.OnInvalidate(invalidation, source);
        }

        #region Subclasses

        private partial class DateLabel : DrawableDate
        {
            public DateLabel(DateTimeOffset date)
                : base(date)
            {
                Font = OsuFont.GetFont(size: 16, weight: FontWeight.Medium, italics: true);
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
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                        },
                        value = new OsuSpriteText
                        {
                            // We don't want the value setting the horizontal size, since it leads to wonky accuracy container length,
                            // since the accuracy is sometimes longer than its name.
                            BypassAutoSizeAxes = Axes.X,
                            Text = statisticInfo.Value,
                            Font = OsuFont.GetFont(size: 19, weight: FontWeight.Medium),
                        }
                    }
                };

                if (score.Combo != score.MaxCombo && statisticInfo.Name == BeatmapsetsStrings.ShowScoreboardHeadersCombo)
                    value.Colour = colours.Lime1;
            }
        }

        private partial class RankLabel : Container, IHasTooltip
        {
            public RankLabel(int? rank)
            {
                if (rank >= 1000)
                    TooltipText = $"#{rank:N0}";

                Child = new OsuSpriteText
                {
                    Shear = -shear,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold, italics: true),
                    Text = rank == null ? "-" : rank.Value.FormatRank().Insert(0, "#")
                };
            }

            public LocalisableString TooltipText { get; }
        }

        private sealed partial class ColouredModSwitchTiny : ModSwitchTiny, IHasTooltip
        {
            private readonly IMod mod;

            public ColouredModSwitchTiny(IMod mod)
                : base(mod)
            {
                this.mod = mod;
                Active.Value = true;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters
                {
                    Roundness = 15,
                    Type = EdgeEffectType.Shadow,
                    Colour = Colour4.Black.Opacity(0.15f),
                    Radius = 3,
                    Offset = new Vector2(-2, 0)
                };
            }

            public LocalisableString TooltipText => (mod as Mod)?.IconTooltip ?? mod.Name;
        }

        #endregion

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (score.Mods.Length > 0 && modsContainer.Any(s => s.IsHovered) && songSelect != null)
                    items.Add(new OsuMenuItem("Use these mods", MenuItemType.Highlighted, () => songSelect.Mods.Value = score.Mods));

                if (score.Files.Count <= 0) return items.ToArray();

                items.Add(new OsuMenuItem("Export", MenuItemType.Standard, () => scoreManager.Export(score)));
                items.Add(new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new LocalScoreDeleteDialog(score))));

                return items.ToArray();
            }
        }
    }
}
