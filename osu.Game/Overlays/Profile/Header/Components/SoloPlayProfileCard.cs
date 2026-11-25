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
    public partial class SoloPlayProfileCard : ProfileHeaderCard
    {
        public readonly IBindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private GlobalRankDisplay globalRankDisplay = null!;
        private ProfileValueDisplay countryRankDisplay = null!;
        private ProfileValueDisplay ppDisplay = null!;
        private ProfileValueDisplay medalsDisplay = null!;
        private RankGraph rankGraph = null!;

        private readonly Dictionary<ScoreRank, ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, ScoreRankInfo>();

        public SoloPlayProfileCard()
            : base(UsersStrings.ShowSoloTitle, OsuIcon.Solo)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            TopContent.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    globalRankDisplay = new GlobalRankDisplay(),
                    countryRankDisplay = new ProfileValueDisplay(ProfileValueDisplay.ValueSize.Medium)
                    {
                        Title = UsersStrings.ShowRankCountrySimple
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                ppDisplay = new ProfileValueDisplay { Title = @"pp" },
                                medalsDisplay = new ProfileValueDisplay { Title = UsersStrings.ShowStatsMedals },
                            },
                        },
                    },
                },
            };

            MiddleContent.Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 60,
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new[]
                    {
                        scoreRankInfos[ScoreRank.XH] = new ScoreRankInfo(ScoreRank.XH),
                        scoreRankInfos[ScoreRank.X] = new ScoreRankInfo(ScoreRank.X),
                        scoreRankInfos[ScoreRank.SH] = new ScoreRankInfo(ScoreRank.SH),
                        scoreRankInfos[ScoreRank.S] = new ScoreRankInfo(ScoreRank.S),
                        scoreRankInfos[ScoreRank.A] = new ScoreRankInfo(ScoreRank.A),
                    },
                },
            };

            BottomContent.Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 80,
                Children = new Drawable[]
                {
                    rankGraph = new RankGraph
                    {
                        RelativeSizeAxes = Axes.Both,
                        GraphPadding = new MarginPadding { Vertical = 10, Right = 15 },
                        Placeholder = UsersStrings.ShowExtraUnranked,
                    },
                },
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

            medalsDisplay.Content.Text = user?.Achievements?.Length.ToString() ?? "0";

            ppDisplay.Content.Text = user?.Statistics?.PP?.ToLocalisableString("#,##0") ?? (LocalisableString)"0";
            ppDisplay.Content.TooltipText = getPPTooltipText(user);

            foreach (var scoreRankInfo in scoreRankInfos)
                scoreRankInfo.Value.RankCount = user?.Statistics?.GradesCount[scoreRankInfo.Key] ?? 0;

            globalRankDisplay.HighestRank.Value = user?.RankHighest;
            globalRankDisplay.UserStatistics.Value = user?.Statistics;

            countryRankDisplay.Content.Text = user?.Statistics?.CountryRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            countryRankDisplay.Content.Font = countryRankDisplay.Content.Font.With(weight: FontWeight.Regular);
            countryRankDisplay.Content.TooltipText = getCountryRankTooltipText(user);

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

        private static LocalisableString getPPTooltipText(APIUser? user)
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
                    Width = 32,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new DrawableRank(rank)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 16,
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
