// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Game.Online.API;

namespace osu.Game.Online.Leaderboards
{
    public abstract partial class LeaderboardScoresProvider<TScope, TScoreInfo> : Component
    {
        /// <summary>
        /// The currently displayed scores.
        /// </summary>
        public IBindableList<TScoreInfo> Scores => scores;

        public TScoreInfo? UserScore { get; private set; }

        private readonly BindableList<TScoreInfo> scores = new BindableList<TScoreInfo>();

        public IBindable<LeaderboardState> State => state;

        private readonly Bindable<LeaderboardState> state = new Bindable<LeaderboardState>();

        /// <summary>
        /// Whether the current scope should refetch in response to changes in API connectivity state.
        /// </summary>
        public abstract bool IsOnlineScope { get; }

        private APIRequest? fetchScoresRequest;

        public Action<CancellationToken>? OnStateChange;

        [Resolved(CanBeNull = true)]
        private IAPIProvider? api { get; set; }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private CancellationTokenSource? currentFetchCancellationSource;

        public Action<TScoreInfo?, CancellationToken, Action>? OnLoadScores;

        private TScope scope = default!;

        public TScope Scope
        {
            get => scope;
            set
            {
                if (EqualityComparer<TScope>.Default.Equals(value, scope))
                    return;

                scope = value;
                RefetchScores();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (api != null)
            {
                apiState.BindTo(api.State);
                apiState.BindValueChanged(state =>
                {
                    switch (state.NewValue)
                    {
                        case APIState.Online:
                        case APIState.Offline:
                            if (IsOnlineScope)
                                RefetchScores();

                            break;
                    }
                });
            }

            RefetchScores();
        }

        /// <summary>
        /// Perform a full refetch of scores using current criteria.
        /// </summary>
        public void RefetchScores() => Scheduler.AddOnce(refetchScores);

        private void refetchScores()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            ClearScores();
            setState(LeaderboardState.Retrieving);

            currentFetchCancellationSource = new CancellationTokenSource();

            fetchScoresRequest = FetchScores(currentFetchCancellationSource.Token);

            if (fetchScoresRequest == null)
                return;

            fetchScoresRequest.Failure += e => Schedule(() =>
            {
                if (e is OperationCanceledException || currentFetchCancellationSource.IsCancellationRequested)
                    return;

                SetErrorState(LeaderboardState.NetworkFailure);
            });

            api?.Queue(fetchScoresRequest);
        }

        /// <summary>
        /// Call when a retrieval or display failure happened to show a relevant message to the user.
        /// </summary>
        /// <param name="state">The state to display.</param>
        protected void SetErrorState(LeaderboardState state)
        {
            switch (state)
            {
                case LeaderboardState.NoScores:
                case LeaderboardState.Retrieving:
                case LeaderboardState.Success:
                    throw new InvalidOperationException($"State {state} cannot be set by a leaderboard implementation.");
            }

            Debug.Assert(!scores.Any());

            setState(state);
        }

        /// <summary>
        /// Performs a fetch/refresh of scores to be displayed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>An <see cref="APIRequest"/> responsible for the fetch operation. This will be queued and performed automatically.</returns>
        protected abstract APIRequest? FetchScores(CancellationToken cancellationToken);

        public void ClearScores()
        {
            cancelPendingWork();
            SetScores(null);
        }

        private void cancelPendingWork()
        {
            currentFetchCancellationSource?.Cancel();
            fetchScoresRequest?.Cancel();
        }


        private void setState(LeaderboardState state)
        {
            if (state == this.state.Value)
                return;

            this.state.Value = state;
        }

        /// <summary>
        /// Call when retrieved scores are ready to be displayed.
        /// </summary>
        /// <param name="scores">The scores to display.</param>
        /// <param name="userScore">The user top score, if any.</param>
        protected void SetScores(IEnumerable<TScoreInfo>? scores, TScoreInfo? userScore = default)
        {
            this.scores.Clear();
            if (scores != null)
                this.scores.AddRange(scores);

            UserScore = userScore;

            if (!this.scores.Any())
                setState(LeaderboardState.NoScores);
            else
                setState(LeaderboardState.Success);
        }
    }
}
