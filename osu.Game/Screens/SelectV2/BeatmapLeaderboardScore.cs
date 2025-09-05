// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osu.Game.Localisation;
using osu.Game.Online.API;
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
    public sealed partial class BeatmapLeaderboardScore : OsuClickableContainer, IHasContextMenu, IHasCustomTooltip<ScoreInfo>
    {
        public const int HEIGHT = 50;

        public readonly ScoreInfo Score;

        public Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>();

        /// <summary>
        /// A function determining whether each mod in the score can be selected.
        /// A return value of <see langword="true"/> means that the mod can be selected in the current context.
        /// A return value of <see langword="false"/> means that the mod cannot be selected in the current context.
        /// </summary>
        public Func<Mod, bool> IsValidMod { get; set; } = _ => true;

        public int? Rank { get; init; }
        public HighlightType? Highlight { get; init; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private Clipboard? clipboard { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private const float expanded_right_content_width = 200;
        private const float grade_width = 35;
        private const float username_min_width = 120;
        private const float statistics_regular_min_width = 165;
        private const float statistics_compact_min_width = 90;
        private const float rank_label_width = 40;

        private const int corner_radius = 10;
        private const int transition_duration = 200;

        private static readonly Color4 personal_best_gradient_left = Color4Extensions.FromHex("#66FFCC");
        private static readonly Color4 personal_best_gradient_right = Color4Extensions.FromHex("#51A388");

        private Colour4 foregroundColour;
        private Colour4 backgroundColour;
        private ColourInfo totalScoreBackgroundGradient;

        private IBindable<ScoringMode> scoringMode { get; set; } = null!;

        private Box background = null!;
        private Box foreground = null!;

        private ClickableAvatar innerAvatar = null!;

        private Container centreContent = null!;
        private Container rightContent = null!;

        private FillFlowContainer<Drawable> modsContainer = null!;

        private Box totalScoreBackground = null!;

        private FillFlowContainer statisticsContainer = null!;
        private Container highlightGradient = null!;
        private Container rankLabelStandalone = null!;
        private Container rankLabelOverlay = null!;

        private readonly bool sheared;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = DrawRectangle;

            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapLeaderboardWedge.SPACING_BETWEEN_SCORES / 2 });

            return inputRectangle.Contains(ToLocalSpace(screenSpacePos));
        }

        public BeatmapLeaderboardScore(ScoreInfo score, bool sheared = true)
        {
            Score = score;

            this.sheared = sheared;

            Shear = sheared ? OsuGame.SHEAR : Vector2.Zero;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foregroundColour = colourProvider.Background5;
            backgroundColour = colourProvider.Background3;
            totalScoreBackgroundGradient = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), backgroundColour);

            Child = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Alpha = 0.4f,
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    rankLabelStandalone = new Container
                    {
                        Width = rank_label_width,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            highlightGradient = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Right = -10f },
                                Alpha = Highlight != null ? 1 : 0,
                                Colour = getHighlightColour(Highlight),
                                Child = new Box { RelativeSizeAxes = Axes.Both },
                            },
                            new RankLabel(Rank, sheared, darkText: Highlight == HighlightType.Own)
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                    },
                    centreContent = new Container
                    {
                        Name = @"Centre container",
                        RelativeSizeAxes = Axes.Both,
                        Child = new Container
                        {
                            Masking = true,
                            CornerRadius = corner_radius,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                foreground = new Box
                                {
                                    Alpha = 0.4f,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = foregroundColour
                                },
                                new UserCoverBackground
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    User = Score.User,
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
                                                Children = new Drawable[]
                                                {
                                                    new DelayedLoadWrapper(innerAvatar = new ClickableAvatar(Score.User)
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
                                                    new FillFlowContainer
                                                    {
                                                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                                        Direction = FillDirection.Horizontal,
                                                        Spacing = new Vector2(5),
                                                        AutoSizeAxes = Axes.Both,
                                                        Masking = true,
                                                        Children = new Drawable[]
                                                        {
                                                            new UpdateableFlag(Score.User.CountryCode)
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                                Size = new Vector2(20, 14),
                                                            },
                                                            new UpdateableTeamFlag(Score.User.Team)
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                                Size = new Vector2(30, 15),
                                                            },
                                                            new DateLabel(Score.Date)
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                                Colour = colourProvider.Content2,
                                                                UseFullGlyphHeight = false,
                                                            }
                                                        }
                                                    },
                                                    new TruncatingSpriteText
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                                        Text = Score.User.Username,
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
                                                    Padding = new MarginPadding { Right = 10 },
                                                    Spacing = new Vector2(20, 0),
                                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Children = new Drawable[]
                                                    {
                                                        new ScoreComponentLabel(BeatmapsetsStrings.ShowScoreboardHeadersCombo.ToUpper(), $"{Score.MaxCombo.ToString()}x",
                                                            Score.MaxCombo == Score.GetMaximumAchievableCombo(), 60),
                                                        new ScoreComponentLabel(BeatmapsetsStrings.ShowScoreboardHeadersAccuracy.ToUpper(), Score.DisplayAccuracy, Score.Accuracy == 1,
                                                            55),
                                                    },
                                                    Alpha = 0,
                                                }
                                            }
                                        }
                                    }
                                },
                            },
                        },
                    },
                    rightContent = new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Name = @"Right content",
                        RelativeSizeAxes = Axes.Y,
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
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
                                        Colour = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), OsuColour.ForRank(Score.Rank)),
                                    },
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = grade_width,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Colour = OsuColour.ForRank(Score.Rank),
                                },
                                new TrianglesV2
                                {
                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    SpawnRatio = 2,
                                    Velocity = 0.7f,
                                    Colour = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), OsuColour.ForRank(Score.Rank).Darken(0.2f)),
                                },
                                new Container
                                {
                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = grade_width,
                                    Child = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Spacing = new Vector2(-2),
                                        Colour = DrawableRank.GetRankNameColour(Score.Rank),
                                        Font = OsuFont.Numeric.With(size: 14),
                                        Text = DrawableRank.GetRankName(Score.Rank),
                                        ShadowColour = Color4.Black.Opacity(0.3f),
                                        ShadowOffset = new Vector2(0, 0.08f),
                                        Shadow = true,
                                        UseFullGlyphHeight = false,
                                    },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Padding = new MarginPadding { Right = grade_width },
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
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
                                                Colour = ColourInfo.GradientHorizontal(backgroundColour.Opacity(0), OsuColour.ForRank(Score.Rank).Opacity(0.5f)),
                                            },
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                Direction = FillDirection.Vertical,
                                                Padding = new MarginPadding { Horizontal = corner_radius },
                                                Spacing = new Vector2(0f, -2f),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                        UseFullGlyphHeight = false,
                                                        Current = scoreManager.GetBindableTotalScoreString(Score),
                                                        Spacing = new Vector2(-1.5f),
                                                        Font = OsuFont.Style.Subtitle.With(weight: FontWeight.Light, fixedWidth: true),
                                                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                                    },
                                                    modsContainer = new FillFlowContainer<Drawable>
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                        Spacing = new Vector2(-10, 0),
                                                        Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                                    },
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };
            innerAvatar.OnLoadComplete += d => d.FadeInFromZero(200);
        }

        private ColourInfo getHighlightColour(HighlightType? highlightType, float lightenAmount = 0)
        {
            switch (highlightType)
            {
                case HighlightType.Own:
                    return ColourInfo.GradientHorizontal(personal_best_gradient_left.Lighten(lightenAmount), personal_best_gradient_right.Lighten(lightenAmount));

                case HighlightType.Friend:
                    return ColourInfo.GradientHorizontal(colours.Pink1.Lighten(lightenAmount), colours.Pink3.Lighten(lightenAmount));

                default:
                    return Colour4.White;
            }
        }

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
            if (Score.Mods.Length > 0)
            {
                modsContainer.Padding = new MarginPadding { Top = 4f };
                modsContainer.ChildrenEnumerable = Score.Mods.AsOrdered().Select(mod => new ModIcon(mod)
                {
                    Scale = new Vector2(0.3f),
                    // trim mod icon height down to its true height for alignment purposes.
                    Height = ModIcon.MOD_ICON_SIZE.Y * 3 / 4f,
                });
            }
        }

        private (CaseTransformableString, LocalisableString DisplayAccuracy)[] getStatistics(ScoreInfo model) => new[]
        {
            (BeatmapsetsStrings.ShowScoreboardHeadersCombo.ToUpper(), model.MaxCombo.ToString().Insert(model.MaxCombo.ToString().Length, "x")),
            (BeatmapsetsStrings.ShowScoreboardHeadersAccuracy.ToUpper(), model.DisplayAccuracy),
        };

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
            highlightGradient.FadeColour(getHighlightColour(Highlight, IsHovered ? 0.2f : 0), transition_duration, Easing.OutQuint);

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
                updateDisplayMode(mode);

            centreContent.Padding = new MarginPadding
            {
                Left = rankLabelStandalone.DrawWidth,
                Right = rightContent.DrawWidth,
            };
        }

        private void updateDisplayMode(DisplayMode mode)
        {
            double duration = currentMode == null ? 0 : transition_duration;
            if (mode >= DisplayMode.Full)
                rankLabelStandalone.FadeIn(duration, Easing.OutQuint).ResizeWidthTo(rank_label_width, duration, Easing.OutQuint);
            else
                rankLabelStandalone.FadeOut(duration, Easing.OutQuint).ResizeWidthTo(0, duration, Easing.OutQuint);

            if (mode >= DisplayMode.Regular)
            {
                statisticsContainer.FadeIn(duration, Easing.OutQuint).MoveToX(0, duration, Easing.OutQuint);
                statisticsContainer.Direction = FillDirection.Horizontal;
                statisticsContainer.ScaleTo(1, duration, Easing.OutQuint);
            }
            else if (mode >= DisplayMode.Compact)
            {
                statisticsContainer.FadeIn(duration, Easing.OutQuint).MoveToX(0, duration, Easing.OutQuint);
                statisticsContainer.Direction = FillDirection.Vertical;
                statisticsContainer.ScaleTo(0.8f, duration, Easing.OutQuint);
            }
            else
                statisticsContainer.FadeOut(duration, Easing.OutQuint).MoveToX(statisticsContainer.DrawWidth, duration, Easing.OutQuint);

            currentMode = mode;
        }

        private DisplayMode getCurrentDisplayMode()
        {
            if (DrawWidth >= username_min_width + statistics_regular_min_width + expanded_right_content_width + rank_label_width)
                return DisplayMode.Full;

            if (DrawWidth >= username_min_width + statistics_regular_min_width + expanded_right_content_width)
                return DisplayMode.Regular;

            if (DrawWidth >= username_min_width + statistics_compact_min_width + expanded_right_content_width)
                return DisplayMode.Compact;

            return DisplayMode.Minimal;
        }

        ITooltip<ScoreInfo> IHasCustomTooltip<ScoreInfo>.GetCustomTooltip() => new LeaderboardScoreTooltip(colourProvider);

        ScoreInfo IHasCustomTooltip<ScoreInfo>.TooltipContent => Score;

        MenuItem[] IHasContextMenu.ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                // system mods should never be copied across regardless of anything.
                var copyableMods = Score.Mods.Where(m => IsValidMod.Invoke(m) && m.Type != ModType.System).ToArray();

                if (copyableMods.Length > 0)
                    items.Add(new OsuMenuItem(SongSelectStrings.UseTheseMods, MenuItemType.Highlighted, () => SelectedMods.Value = copyableMods));

                if (Score.OnlineID > 0)
                    items.Add(new OsuMenuItem(CommonStrings.CopyLink, MenuItemType.Standard, () => clipboard?.SetText($@"{api.Endpoints.WebsiteUrl}/scores/{Score.OnlineID}")));

                if (Score.Files.Count <= 0) return items.ToArray();

                items.Add(new OsuMenuItem(CommonStrings.Export, MenuItemType.Standard, () => scoreManager.Export(Score)));
                items.Add(new OsuMenuItem(Resources.Localisation.Web.CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new LocalScoreDeleteDialog(Score))));

                return items.ToArray();
            }
        }

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

            protected override LocalisableString Format() => Date.ToShortRelativeTime(TimeSpan.FromSeconds(30));
        }

        private partial class ScoreComponentLabel : Container
        {
            private readonly LocalisableString name;
            private readonly LocalisableString value;
            private readonly bool perfect;
            private readonly float minWidth;

            private FillFlowContainer content = null!;
            public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

            public ScoreComponentLabel(LocalisableString name, LocalisableString value, bool perfect, float minWidth)
            {
                this.name = name;
                this.value = value;
                this.perfect = perfect;
                this.minWidth = minWidth;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                Child = content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Colour = colourProvider.Content2,
                            Text = name,
                            Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                        },
                        new OsuSpriteText
                        {
                            // We don't want the value setting the horizontal size, since it leads to wonky accuracy container length,
                            // since the accuracy is sometimes longer than its name.
                            BypassAutoSizeAxes = Axes.X,
                            Text = value,
                            Font = OsuFont.Style.Body,
                            Colour = perfect ? colours.Lime1 : Color4.White,
                        },
                        Empty().With(d => d.Width = minWidth),
                    }
                };
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
                    Text = rank?.FormatRank().Insert(0, "#") ?? "-",
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

        public enum HighlightType
        {
            Own,
            Friend,
        }
    }
}
