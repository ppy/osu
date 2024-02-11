// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.Solo
{
    /// <summary>
    /// A persistent component that binds to the spectator server and API in order to deliver updates about the logged in user's gameplay statistics.
    /// </summary>
    public partial class SoloStatisticsWatcher : Component
    {
        private readonly LocalUserStatisticsProvider? statisticsProvider;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Dictionary<long, StatisticsUpdateCallback> callbacks = new Dictionary<long, StatisticsUpdateCallback>();
        private long? lastProcessedScoreId;

        private Dictionary<string, UserStatistics>? latestStatistics;

        public SoloStatisticsWatcher(LocalUserStatisticsProvider? statisticsProvider = null)
        {
            this.statisticsProvider = statisticsProvider;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            api.LocalUser.BindValueChanged(user => onUserChanged(user.NewValue), true);
            spectatorClient.OnUserScoreProcessed += userScoreProcessed;
        }

        /// <summary>
        /// Registers for a user statistics update after the given <paramref name="score"/> has been processed server-side.
        /// </summary>
        /// <param name="score">The score to listen for the statistics update for.</param>
        /// <param name="onUpdateReady">The callback to be invoked once the statistics update has been prepared.</param>
        /// <returns>An <see cref="IDisposable"/> representing the subscription. Disposing it is equivalent to unsubscribing from future notifications.</returns>
        public IDisposable RegisterForStatisticsUpdateAfter(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady)
        {
            Schedule(() =>
            {
                if (!api.IsLoggedIn)
                    return;

                if (!score.Ruleset.IsLegacyRuleset() || score.OnlineID <= 0)
                    return;

                var callback = new StatisticsUpdateCallback(score, onUpdateReady);

                if (lastProcessedScoreId == score.OnlineID)
                {
                    requestStatisticsUpdate(api.LocalUser.Value.Id, callback);
                    return;
                }

                callbacks.Add(score.OnlineID, callback);
            });

            return new InvokeOnDisposal(() => Schedule(() => callbacks.Remove(score.OnlineID)));
        }

        private void onUserChanged(APIUser? localUser) => Schedule(() =>
        {
            callbacks.Clear();
            lastProcessedScoreId = null;
            latestStatistics = null;

            if (localUser == null || localUser.OnlineID <= 1)
                return;

            var userRequest = new GetUsersRequest(new[] { localUser.OnlineID });
            userRequest.Success += initialiseUserStatistics;
            api.Queue(userRequest);
        });

        private void initialiseUserStatistics(GetUsersResponse response) => Schedule(() =>
        {
            var user = response.Users.SingleOrDefault();

            // possible if the user is restricted or similar.
            if (user == null)
                return;

            latestStatistics = new Dictionary<string, UserStatistics>();

            if (user.RulesetsStatistics != null)
            {
                foreach (var rulesetStats in user.RulesetsStatistics)
                    latestStatistics.Add(rulesetStats.Key, rulesetStats.Value);
            }
        });

        private void userScoreProcessed(int userId, long scoreId)
        {
            if (userId != api.LocalUser.Value?.OnlineID)
                return;

            lastProcessedScoreId = scoreId;

            if (!callbacks.TryGetValue(scoreId, out var callback))
                return;

            requestStatisticsUpdate(userId, callback);
            callbacks.Remove(scoreId);
        }

        private void requestStatisticsUpdate(int userId, StatisticsUpdateCallback callback)
        {
            var request = new GetUserRequest(userId, callback.Score.Ruleset);
            request.Success += user => Schedule(() => dispatchStatisticsUpdate(callback, user.Statistics));
            api.Queue(request);
        }

        private void dispatchStatisticsUpdate(StatisticsUpdateCallback callback, UserStatistics updatedStatistics)
        {
            string rulesetName = callback.Score.Ruleset.ShortName;

            statisticsProvider?.UpdateStatistics(updatedStatistics, callback.Score.Ruleset);

            if (latestStatistics == null)
                return;

            latestStatistics.TryGetValue(rulesetName, out UserStatistics? latestRulesetStatistics);
            latestRulesetStatistics ??= new UserStatistics();

            var update = new SoloStatisticsUpdate(callback.Score, latestRulesetStatistics, updatedStatistics);
            callback.OnUpdateReady.Invoke(update);

            latestStatistics[rulesetName] = updatedStatistics;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (spectatorClient.IsNotNull())
                spectatorClient.OnUserScoreProcessed -= userScoreProcessed;

            base.Dispose(isDisposing);
        }

        private class StatisticsUpdateCallback
        {
            public ScoreInfo Score { get; }
            public Action<SoloStatisticsUpdate> OnUpdateReady { get; }

            public StatisticsUpdateCallback(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady)
            {
                Score = score;
                OnUpdateReady = onUpdateReady;
            }
        }
    }
}
