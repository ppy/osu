// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class MatchmakingStatsDisplay : CompositeDrawable, IHasCustomTooltip<MatchmakingStatsTooltipData>
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private OsuSpriteText rankText = null!;

        public MatchmakingStatsDisplay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 6,
                    BorderThickness = 2,
                    BorderColour = colourProvider.Background4,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(3f),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = UsersStrings.ShowMatchmakingTitle,
                                    Margin = new MarginPadding { Horizontal = 5f, Vertical = 7f },
                                    Font = OsuFont.GetFont(size: 12)
                                },
                                new Container
                                {
                                    AutoSizeAxes = Axes.X,
                                    RelativeSizeAxes = Axes.Y,
                                    CornerRadius = 3,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background6,
                                        },
                                        rankText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            UseFullGlyphHeight = false,
                                            Colour = colourProvider.Content2,
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f }
                                        },
                                    }
                                },
                            }
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            if (User.Value == null)
            {
                Hide();
                return;
            }

            APIUserMatchmakingStatistics[] allStats = User.Value.User.MatchmakingStatistics;

            if (allStats.Length == 0)
            {
                Hide();
                return;
            }

            APIUserMatchmakingStatistics? highestRankStats = null;

            foreach (var stats in allStats)
            {
                if (stats.Pool.Active && (highestRankStats == null || stats.Rank < highestRankStats.Rank))
                    highestRankStats = stats;
            }

            rankText.Text = highestRankStats == null ? "-" : $"#{highestRankStats.Rank:N0}";

            if (highestRankStats != null)
                rankText.Colour = OsuColour.ForRankingTier(GetRankingTier(highestRankStats));

            TooltipContent = new MatchmakingStatsTooltipData(colourProvider, allStats.OrderByDescending(s => s.PoolId).ToArray());

            Show();
        }

        /// <seealso href="https://github.com/ppy/osu-web/blob/9f136df53a1c436229b0e4eb192011c15514dcf9/resources/js/profile-page/matchmaking.tsx#L15-L34"/>
        public static RankingTier GetRankingTier(APIUserMatchmakingStatistics stats)
        {
            int rank = stats.Rank;
            float percent = stats.RankPercent;

            if (rank <= 100)
                return RankingTier.Lustrous;

            if (percent < 0.05)
                return RankingTier.Radiant;

            if (percent < 0.2)
                return RankingTier.Rhodium;

            if (percent < 0.5)
                return RankingTier.Platinum;

            if (percent < 0.75)
                return RankingTier.Gold;

            if (percent < 0.95)
                return RankingTier.Silver;

            return RankingTier.Bronze;
        }

        public ITooltip<MatchmakingStatsTooltipData> GetCustomTooltip() => new MatchmakingStatsTooltip();

        public MatchmakingStatsTooltipData? TooltipContent { get; private set; }
    }
}
