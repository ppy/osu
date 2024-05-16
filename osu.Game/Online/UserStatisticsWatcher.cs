// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
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
        public IBindable<UserStatisticsUpdate?> LatestUpdate => latestUpdate;
        private readonly Bindable<UserStatisticsUpdate?> latestUpdate = new Bindable<UserStatisticsUpdate?>();

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Dictionary<long, ScoreInfo> watchedScores = new Dictionary<long, ScoreInfo>();

        private Dictionary<string, UserStatistics>? latestStatistics;

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

        private void onUserChanged(APIUser? localUser) => Schedule(() =>
        {
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
            string rulesetName = scoreInfo.Ruleset.ShortName;

            api.UpdateStatistics(updatedStatistics);

            if (latestStatistics == null)
                return;

            latestStatistics.TryGetValue(rulesetName, out UserStatistics? latestRulesetStatistics);
            latestRulesetStatistics ??= new UserStatistics();

            latestUpdate.Value = new UserStatisticsUpdate(scoreInfo, latestRulesetStatistics, updatedStatistics);
            latestStatistics[rulesetName] = updatedStatistics;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (spectatorClient.IsNotNull())
                spectatorClient.OnUserScoreProcessed -= userScoreProcessed;

            base.Dispose(isDisposing);
        }
    }
}
