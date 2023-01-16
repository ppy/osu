// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderBoardScoreV2 : OsuClickableContainer, IHasContextMenu
    {
        private readonly ScoreInfo score;

        private const int HEIGHT = 60;
        private const int corner_radius = 10;
        private const int transition_duration = 200;

        private readonly int? rank;

        private readonly bool isPersonalBest;

        private Colour4 foregroundColour;
        private Colour4 backgroundColour;

        private static readonly Vector2 shear = new Vector2(0.15f, 0);

        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private Storage storage { get; set; } = null!;

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

        private OsuSpriteText scoreText = null!;
        private Drawable scoreRank = null!;

        public LeaderBoardScoreV2(ScoreInfo score, int? rank, bool isPersonalBest = false)
        {
            this.score = score;
            this.rank = rank;
            this.isPersonalBest = isPersonalBest;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scoreManager)
        {
            var user = score.User;

            foregroundColour = isPersonalBest ? colourProvider.Background1 : colourProvider.Background5;
            backgroundColour = isPersonalBest ? colourProvider.Background2 : colourProvider.Background4;

            statisticsLabels = GetStatistics(score).Select(s => new ScoreComponentLabel(s, score)).ToList();

            Shear = shear;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
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
                            new Dimension(GridSizeMode.Absolute, 65),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 176)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new RankLabel(rank)
                                {
                                    Shear = -shear,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 35
                                },
                                createCentreContent(user),
                                createRightSideContent(scoreManager)
                            }
                        }
                    }
                }
            };

            innerAvatar.OnLoadComplete += d => d.FadeInFromZero(200);

            modsContainer.Spacing = new Vector2(modsContainer.Children.Count > 5 ? -20 : 2, 0);
            modsContainer.Padding = new MarginPadding { Top = modsContainer.Children.Count > 0 ? 4 : 0 };
        }

        private Container createCentreContent(APIUser user) =>
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    foreground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = foregroundColour
                    },
                    avatar = new MaskedWrapper(
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
                        Size = new Vector2(HEIGHT)
                    },
                    new FillFlowContainer
                    {
                        Position = new Vector2(HEIGHT + 9, 9),
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            flagBadgeAndDateContainer = new FillFlowContainer
                            {
                                Shear = -shear,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5f, 0f),
                                Size = new Vector2(87, 16),
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
                                    }
                                }
                            },
                            nameLabel = new OsuSpriteText
                            {
                                Shear = -shear,
                                Text = user.Username,
                                Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold)
                            }
                        }
                    },
                }
            };

        private FillFlowContainer createRightSideContent(ScoreManager scoreManager) =>
            new FillFlowContainer
            {
                Padding = new MarginPadding { Left = 11, Right = 15 },
                Y = -5,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(13, 0f),
                Children = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            scoreText = new OsuSpriteText
                            {
                                Shear = -shear,
                                Current = scoreManager.GetBindableTotalScoreString(score),

                                //Does not match figma, adjusted to allow 8 digits to fit comfortably
                                Font = OsuFont.GetFont(size: 28, weight: FontWeight.SemiBold, fixedWidth: false),
                            },
                            RankContainer = new Container
                            {
                                BypassAutoSizeAxes = Axes.Both,
                                Y = 2,
                                Shear = -shear,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                AutoSizeAxes = Axes.Both,
                                Children = new[]
                                {
                                    scoreRank = new UpdateableRank(score.Rank)
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(32)
                                    }
                                }
                            }
                        }
                    },
                    modsContainer = new FillFlowContainer<ColouredModSwitchTiny>
                    {
                        Shear = -shear,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        ChildrenEnumerable = score.Mods.Select(mod => new ColouredModSwitchTiny(mod) { Scale = new Vector2(0.375f) })
                    }
                }
            };

        protected (CaseTransformableString, LocalisableString DisplayAccuracy)[] GetStatistics(ScoreInfo model) => new[]
        {
            (EditorSetupStrings.ComboColourPrefix.ToUpper(), model.MaxCombo.ToString().Insert(model.MaxCombo.ToString().Length, "x")),
            (BeatmapsetsStrings.ShowScoreboardHeadersAccuracy.ToUpper(), model.DisplayAccuracy),
            (getResultNames(score).ToUpper(), getResults(score).ToUpper())
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
            foreground.FadeColour(IsHovered ? foregroundColour.Lighten(0.2f) : foregroundColour, transition_duration, Easing.OutQuint);
            background.FadeColour(IsHovered ? backgroundColour.Lighten(0.2f) : backgroundColour, transition_duration, Easing.OutQuint);
        }

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
                    Padding = new MarginPadding { Right = 25 },
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
                            Text = statisticInfo.Value,
                            Font = OsuFont.GetFont(size: 19, weight: FontWeight.Medium),
                        }
                    }
                };

                if (score.Combo == score.MaxCombo && statisticInfo.Name == EditorSetupStrings.ComboColourPrefix.ToUpper())
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
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold, italics: true),
                    Text = rank == null ? "-" : rank.Value.FormatRank().Insert(0, "#")
                };
            }

            public LocalisableString TooltipText { get; }
        }

        private partial class MaskedWrapper : DelayedLoadWrapper
        {
            public MaskedWrapper(Drawable content, double timeBeforeLoad = 500)
                : base(content, timeBeforeLoad)
            {
                CornerRadius = corner_radius;
                Masking = true;
            }
        }

        private partial class ColouredModSwitchTiny : ModSwitchTiny
        {
            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public ColouredModSwitchTiny(IMod mod)
                : base(mod)
            {
            }

            protected override void UpdateState()
            {
                AcronymText.Colour = Colour4.FromHex("#555555");
                Background.Colour = colours.Yellow;
            }
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (score.Mods.Length > 0 && modsContainer.Any(s => s.IsHovered) && songSelect != null)
                    items.Add(new OsuMenuItem("Use these mods", MenuItemType.Highlighted, () => songSelect.Mods.Value = score.Mods));

                if (score.Files.Count <= 0) return items.ToArray();

                items.Add(new OsuMenuItem("Export", MenuItemType.Standard, () => new LegacyScoreExporter(storage).Export(score)));
                items.Add(new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new LocalScoreDeleteDialog(score))));

                return items.ToArray();
            }
        }

        private LocalisableString getResults(ScoreInfo score)
        {
            string resultString = score.GetStatisticsForDisplay().Where(s => s.Result.IsBasic()).Aggregate(string.Empty, (current, result) =>
                current.Insert(current.Length, $"{result.Count}/"));

            return resultString.Remove(resultString.Length - 1);
        }

        private LocalisableString getResultNames(ScoreInfo score)
        {
            string resultName = string.Empty;

            foreach (var hitResult in score.GetStatisticsForDisplay().Where(s => s.Result.IsBasic()))
            {
                switch (hitResult.Result)
                {
                    case HitResult.Perfect:
                        appendToString("320/");
                        break;

                    case HitResult.Great:
                        appendToString("300/");
                        break;

                    case HitResult.Good:
                        appendToString("200/");
                        break;

                    case HitResult.Ok:
                        appendToString("100/");
                        break;

                    case HitResult.Meh:
                        appendToString("50/");
                        break;

                    case HitResult.Miss:
                        appendToString("X");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            void appendToString(string appendedString) => resultName = resultName.Insert(resultName.Length, appendedString);

            return resultName.Remove(resultName.Length);
        }
    }
}
