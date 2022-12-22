// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
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
        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Dictionary<long, StatisticsUpdateCallback> callbacks = new Dictionary<long, StatisticsUpdateCallback>();
        private readonly HashSet<long> scoresWithoutCallback = new HashSet<long>();

        private readonly Dictionary<string, UserStatistics> latestStatistics = new Dictionary<string, UserStatistics>();

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
        public void RegisterForStatisticsUpdateAfter(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady) => Schedule(() =>
        {
            if (!api.IsLoggedIn)
                return;

            var callback = new StatisticsUpdateCallback(score, onUpdateReady);

            if (scoresWithoutCallback.Remove(score.OnlineID))
            {
                requestStatisticsUpdate(api.LocalUser.Value.Id, callback);
                return;
            }

            callbacks[score.OnlineID] = callback;
        });

        private void onUserChanged(APIUser? localUser) => Schedule(() =>
        {
            callbacks.Clear();
            scoresWithoutCallback.Clear();
            latestStatistics.Clear();

            if (!api.IsLoggedIn)
                return;

            Debug.Assert(localUser != null);

            var userRequest = new GetUsersRequest(new[] { localUser.OnlineID });
            userRequest.Success += response => Schedule(() =>
            {
                foreach (var rulesetStats in response.Users.Single().RulesetsStatistics)
                    latestStatistics.Add(rulesetStats.Key, rulesetStats.Value);
            });
            api.Queue(userRequest);
        });

        private void userScoreProcessed(int userId, long scoreId)
        {
            if (userId != api.LocalUser.Value?.OnlineID)
                return;

            if (!callbacks.TryGetValue(scoreId, out var callback))
            {
                scoresWithoutCallback.Add(scoreId);
                return;
            }

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

            if (!latestStatistics.TryGetValue(rulesetName, out var latestRulesetStatistics))
                return;

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
