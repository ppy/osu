// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class DetailHeaderContainer : CompositeDrawable
    {
        private readonly Dictionary<ScoreRank, ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, ScoreRankInfo>();
        private OverlinedInfoContainer medalInfo;
        private OverlinedInfoContainer ppInfo;
        private OverlinedInfoContainer detailGlobalRank;
        private OverlinedInfoContainer detailCountryRank;
        private FillFlowContainer fillFlow;
        private RankGraph rankGraph;

        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        private bool expanded = true;

        public bool Expanded
        {
            set
            {
                if (expanded == value) return;

                expanded = value;

                if (fillFlow == null) return;

                fillFlow.ClearTransforms();

                if (expanded)
                    fillFlow.AutoSizeAxes = Axes.Y;
                else
                {
                    fillFlow.AutoSizeAxes = Axes.None;
                    fillFlow.ResizeHeightTo(0, 200, Easing.OutQuint);
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
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                fillFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = expanded ? Axes.Y : Axes.None,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        new OverlinedTotalPlayTime
                                        {
                                            User = { BindTarget = User }
                                        },
                                        medalInfo = new OverlinedInfoContainer
                                        {
                                            Title = UsersStrings.ShowStatsMedals,
                                            LineColour = colours.GreenLight,
                                        },
                                        ppInfo = new OverlinedInfoContainer
                                        {
                                            Title = "pp",
                                            LineColour = colours.Red,
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5),
                                    Children = new[]
                                    {
                                        scoreRankInfos[ScoreRank.XH] = new ScoreRankInfo(ScoreRank.XH),
                                        scoreRankInfos[ScoreRank.X] = new ScoreRankInfo(ScoreRank.X),
                                        scoreRankInfos[ScoreRank.SH] = new ScoreRankInfo(ScoreRank.SH),
                                        scoreRankInfos[ScoreRank.S] = new ScoreRankInfo(ScoreRank.S),
                                        scoreRankInfos[ScoreRank.A] = new ScoreRankInfo(ScoreRank.A),
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = 130 },
                            Children = new Drawable[]
                            {
                                rankGraph = new RankGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Y,
                                    Width = 130,
                                    Anchor = Anchor.TopRight,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Horizontal = 10 },
                                    Spacing = new Vector2(0, 20),
                                    Children = new Drawable[]
                                    {
                                        detailGlobalRank = new OverlinedInfoContainer(true, 110)
                                        {
                                            Title = UsersStrings.ShowRankGlobalSimple,
                                            LineColour = colourProvider.Highlight1,
                                        },
                                        detailCountryRank = new OverlinedInfoContainer(false, 110)
                                        {
                                            Title = UsersStrings.ShowRankCountrySimple,
                                            LineColour = colourProvider.Highlight1,
                                        },
                                    }
                                }
                            }
                        },
                    }
                },
            };
        }

        private void updateDisplay(APIUser user)
        {
            medalInfo.Content = user?.Achievements?.Length.ToString() ?? "0";
            ppInfo.Content = user?.Statistics?.PP?.ToLocalisableString("#,##0") ?? (LocalisableString)"0";

            foreach (var scoreRankInfo in scoreRankInfos)
                scoreRankInfo.Value.RankCount = user?.Statistics?.GradesCount[scoreRankInfo.Key] ?? 0;

            detailGlobalRank.Content = user?.Statistics?.GlobalRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            detailCountryRank.Content = user?.Statistics?.CountryRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";

            rankGraph.Statistics.Value = user?.Statistics;
        }

        private class ScoreRankInfo : CompositeDrawable
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
