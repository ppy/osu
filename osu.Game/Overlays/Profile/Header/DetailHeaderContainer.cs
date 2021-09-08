// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class DetailHeaderContainer : CompositeDrawable
    {
        private OverlinedInfoContainer detailGlobalRank;
        private OverlinedInfoContainer detailCountryRank;
        private FillFlowContainer fillFlow;
        private RankGraph rankGraph;

        public readonly Bindable<User> User = new Bindable<User>();
        public readonly BindableBool DetailsVisible = new BindableBool(true);

        private bool expanded = true;
        private ComponentContainer rankGraphContainer;
        private ComponentContainer rankInfoContainer;
        private FillFlowContainer expandedInfoFillFlow;
        private FillFlowContainer hiddenInfoFillFlow;
        private OverlinedInfoContainer hiddenDetailGlobal;
        private OverlinedInfoContainer hiddenDetailCountry;
        private OsuClickableContainer toggleFoldButton;

        public bool Expanded
        {
            set
            {
                if (expanded == value) return;

                expanded = value;

                if (fillFlow == null) return;

                fillFlow.ClearTransforms();

                if (expanded)
                {
                    expandedInfoFillFlow.FadeIn(300, Easing.OutQuint);
                    hiddenInfoFillFlow.FadeOut(300, Easing.OutQuint);
                    toggleFoldButton.TooltipText = "折叠";
                }
                else
                {
                    expandedInfoFillFlow.FadeOut(300, Easing.OutQuint);
                    hiddenInfoFillFlow.FadeIn(300, Easing.OutQuint);
                    toggleFoldButton.TooltipText = "展开";
                }
            }
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            AutoSizeAxes = Axes.Y;

            User.ValueChanged += e => updateDisplay(e.NewValue);

            InternalChildren = new Drawable[]
            {
                fillFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Bottom = 35 },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            CornerRadius = 25,
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Depth = float.MaxValue,
                                    Colour = colourProvider.Background4,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                rankGraphContainer = new ComponentContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Vertical = 15 },
                                        Child = rankGraph = new RankGraph
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                    }
                                },
                                rankInfoContainer = new ComponentContainer
                                {
                                    Name = "Rank Info Container",
                                    AutoSizeDuration = 200,
                                    AutoSizeEasing = Easing.OutQuint,
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background6,
                                        },
                                        expandedInfoFillFlow = new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Vertical,
                                            Padding = new MarginPadding(25),
                                            Spacing = new Vector2(0, 10),
                                            Children = new Drawable[]
                                            {
                                                detailGlobalRank = new OverlinedInfoContainer(true, 110)
                                                {
                                                    Title = UsersStrings.ShowRankGlobalSimple,
                                                },
                                                detailCountryRank = new OverlinedInfoContainer(false, 110)
                                                {
                                                    Title = UsersStrings.ShowRankCountrySimple,
                                                },
                                            }
                                        },
                                        hiddenInfoFillFlow = new FillFlowContainer
                                        {
                                            Name = "Hidden Info Container",
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Alpha = 0,
                                            LayoutDuration = 200,
                                            LayoutEasing = Easing.OutQuint,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Padding = new MarginPadding { Horizontal = 25, Vertical = 15 },
                                            Spacing = new Vector2(10),
                                            Children = new[]
                                            {
                                                hiddenDetailGlobal = new OverlinedInfoContainer(false, 60, FillDirection.Horizontal)
                                                {
                                                    Title = UsersStrings.ShowRankGlobalSimple,
                                                },
                                                hiddenDetailCountry = new OverlinedInfoContainer(false, 60, FillDirection.Horizontal)
                                                {
                                                    Title = UsersStrings.ShowRankCountrySimple,
                                                },
                                            }
                                        },
                                        toggleFoldButton = new OsuClickableContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Action = () => DetailsVisible.Toggle(),
                                            TooltipText = expanded ? CommonStrings.ButtonsCollapse : CommonStrings.ButtonsExpand
                                        }
                                    }
                                },
                            }
                        },
                    }
                },
            };
        }

        private void updateDisplay(User user)
        {
            detailGlobalRank.Content = user?.Statistics?.GlobalRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            detailCountryRank.Content = user?.Statistics?.CountryRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";

            hiddenDetailGlobal.Content = user?.Statistics?.GlobalRank?.ToString("\\##,##0") ?? "-";
            hiddenDetailCountry.Content = user?.Statistics?.CountryRank?.ToString("\\##,##0") ?? "-";

            rankGraph.Statistics.Value = user?.Statistics;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            rankGraphContainer.Padding = new MarginPadding { Left = 25, Right = rankInfoContainer.Width + 25 };
        }

        public class ScoreRankInfo : CompositeDrawable
        {
            private readonly OsuSpriteText rankCount;

            public int RankCount
            {
                set => rankCount.Text = value.ToLocalisableString("#,##0");
            }

            public ScoreRankInfo(ScoreRank rank)
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Width = 56,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new DrawableRank(rank)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                        },
                        rankCount = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        }
                    }
                };
            }
        }
    }
}
