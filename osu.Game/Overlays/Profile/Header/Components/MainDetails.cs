// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class MainDetails : CompositeDrawable
    {
        private readonly Dictionary<ScoreRank, ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, ScoreRankInfo>();
        private ProfileValueDisplay medalInfo = null!;
        private ProfileValueDisplay ppInfo = null!;
        private GlobalRankDisplay detailGlobalRank = null!;
        private ProfileValueDisplay detailCountryRank = null!;
        private RankGraph rankGraph = null!;

        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeDuration = 200,
                AutoSizeEasing = Easing.OutQuint,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 15),
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 20),
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                detailGlobalRank = new GlobalRankDisplay(),
                                Empty(),
                                detailCountryRank = new ProfileValueDisplay(true)
                                {
                                    Title = UsersStrings.ShowRankCountrySimple,
                                },
                                new DailyChallengeStatsDisplay
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    User = { BindTarget = User },
                                }
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 60,
                        Children = new Drawable[]
                        {
                            rankGraph = new RankGraph
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
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
                                    medalInfo = new ProfileValueDisplay
                                    {
                                        Title = UsersStrings.ShowStatsMedals,
                                    },
                                    ppInfo = new ProfileValueDisplay
                                    {
                                        Title = "pp",
                                    },
                                    new TotalPlayTime
                                    {
                                        User = { BindTarget = User }
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
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(e => updateDisplay(e.NewValue), true);
        }

        private void updateDisplay(UserProfileData? data)
        {
            var user = data?.User;

            medalInfo.Content.Text = user?.Achievements?.Length.ToString() ?? "0";
            ppInfo.Content.Text = user?.Statistics?.PP?.ToLocalisableString("#,##0") ?? (LocalisableString)"0";
            ppInfo.Content.TooltipText = getPPInfoTooltipText(user);

            foreach (var scoreRankInfo in scoreRankInfos)
                scoreRankInfo.Value.RankCount = user?.Statistics?.GradesCount[scoreRankInfo.Key] ?? 0;

            detailGlobalRank.HighestRank.Value = user?.RankHighest;
            detailGlobalRank.UserStatistics.Value = user?.Statistics;

            detailCountryRank.Content.Text = user?.Statistics?.CountryRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            detailCountryRank.Content.TooltipText = getCountryRankTooltipText(user);

            rankGraph.Statistics.Value = user?.Statistics;
        }

        private static LocalisableString getCountryRankTooltipText(APIUser? user)
        {
            var variants = user?.Statistics?.Variants;

            LocalisableString? result = null;

            if (variants?.Count > 0)
            {
                foreach (var variant in variants)
                {
                    if (variant.CountryRank != null)
                    {
                        var variantText = LocalisableString.Interpolate($"{variant.VariantType.GetLocalisableDescription()}: {variant.CountryRank.ToLocalisableString("\\##,##0")}");

                        if (result == null)
                            result = variantText;
                        else
                            result = LocalisableString.Interpolate($"{result}\n{variantText}");
                    }
                }
            }

            return result ?? default;
        }

        private static LocalisableString getPPInfoTooltipText(APIUser? user)
        {
            var variants = user?.Statistics?.Variants;

            LocalisableString? result = null;

            if (variants?.Count > 0)
            {
                foreach (var variant in variants)
                {
                    var variantText = LocalisableString.Interpolate($"{variant.VariantType.GetLocalisableDescription()}: {variant.PP.ToLocalisableString("#,##0")}");

                    if (result == null)
                        result = variantText;
                    else
                        result = LocalisableString.Interpolate($"{result}\n{variantText}");
                }
            }

            return result ?? default;
        }

        private partial class ScoreRankInfo : CompositeDrawable
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
                    Width = 44,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new DrawableRank(rank)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 22,
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
