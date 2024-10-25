// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online
{
    /// <summary>
    /// A persistent component that binds to the spectator server and API in order to deliver updates about the logged in user's gameplay statistics.
    /// </summary>
    public partial class UserStatisticsWatcher : Component
    {
        private readonly LocalUserStatisticsProvider? statisticsProvider;
        public IBindable<UserStatisticsUpdate?> LatestUpdate => latestUpdate;
        private readonly Bindable<UserStatisticsUpdate?> latestUpdate = new Bindable<UserStatisticsUpdate?>();

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Dictionary<long, ScoreInfo> watchedScores = new Dictionary<long, ScoreInfo>();

        public UserStatisticsWatcher(LocalUserStatisticsProvider? statisticsProvider = null)
        {
            this.statisticsProvider = statisticsProvider;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            spectatorClient.OnUserScoreProcessed += userScoreProcessed;
        }

        /// <summary>
        /// Registers for a user statistics update after the given <paramref name="score"/> has been processed server-side.
        /// </summary>
        /// <param name="score">The score to listen for the statistics update for.</param>
        public void RegisterForStatisticsUpdateAfter(ScoreInfo score)
        {
            Schedule(() =>
            {
                if (!api.IsLoggedIn)
                    return;

                if (!score.Ruleset.IsLegacyRuleset() || score.OnlineID <= 0)
                    return;

                watchedScores.Add(score.OnlineID, score);
            });
        }

        private void userScoreProcessed(int userId, long scoreId)
        {
            if (userId != api.LocalUser.Value?.OnlineID)
                return;

            if (!watchedScores.Remove(scoreId, out var scoreInfo))
                return;

            requestStatisticsUpdate(userId, scoreInfo);
        }

        private void requestStatisticsUpdate(int userId, ScoreInfo scoreInfo)
        {
            var request = new GetUserRequest(userId, scoreInfo.Ruleset);
            request.Success += user => Schedule(() => dispatchStatisticsUpdate(scoreInfo, user.Statistics));
            api.Queue(request);
        }

        private void dispatchStatisticsUpdate(ScoreInfo scoreInfo, UserStatistics updatedStatistics)
        {
            if (statisticsProvider == null)
                return;

            var latestRulesetStatistics = statisticsProvider.GetStatisticsFor(scoreInfo.Ruleset);
            statisticsProvider.UpdateStatistics(updatedStatistics, scoreInfo.Ruleset);

            if (latestRulesetStatistics != null)
                latestUpdate.Value = new UserStatisticsUpdate(scoreInfo, latestRulesetStatistics, updatedStatistics);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (spectatorClient.IsNotNull())
                spectatorClient.OnUserScoreProcessed -= userScoreProcessed;

            base.Dispose(isDisposing);
        }
    }
}
