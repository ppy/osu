// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.Solo;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics.User;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class UserStatisticsPanel : StatisticsPanel
    {
        private readonly ScoreInfo achievedScore;

        internal readonly Bindable<UserStatisticsUpdate?> DisplayedUserStatisticsUpdate = new Bindable<UserStatisticsUpdate?>();

        private IBindable<UserStatisticsUpdate?> latestGlobalStatisticsUpdate = null!;

        public UserStatisticsPanel(ScoreInfo achievedScore)
        {
            this.achievedScore = achievedScore;
        }

        [BackgroundDependencyLoader]
        private void load(UserStatisticsWatcher? soloStatisticsWatcher)
        {
            if (soloStatisticsWatcher != null)
            {
                latestGlobalStatisticsUpdate = soloStatisticsWatcher.LatestUpdate.GetBoundCopy();
                latestGlobalStatisticsUpdate.BindValueChanged(update =>
                {
                    if (update.NewValue?.Score.MatchesOnlineID(achievedScore) == true)
                        DisplayedUserStatisticsUpdate.Value = update.NewValue;
                });
            }
        }

        protected override ICollection<StatisticItem> CreateStatisticItems(ScoreInfo newScore, IBeatmap playableBeatmap)
        {
            var items = base.CreateStatisticItems(newScore, playableBeatmap);

            if (newScore.UserID > 1
                && newScore.UserID == achievedScore.UserID
                && newScore.OnlineID > 0
                && newScore.OnlineID == achievedScore.OnlineID)
            {
                items = items.Append(new StatisticItem("Overall Ranking", () => new OverallRanking
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    StatisticsUpdate = { BindTarget = DisplayedUserStatisticsUpdate }
                })).ToArray();
            }

            return items;
        }
    }
}
