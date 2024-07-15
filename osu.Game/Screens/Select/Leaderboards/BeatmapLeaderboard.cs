// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Screens.Select.Leaderboards
{
    public partial class BeatmapLeaderboard : Leaderboard<BeatmapLeaderboardScope, ScoreInfo>
    {
        public Action<ScoreInfo>? ScoreSelected;

        private BeatmapInfo? beatmapInfo;

        public BeatmapInfo? BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (beatmapInfo == null && value == null)
                    return;

                if (beatmapInfo?.Equals(value) == true)
                    return;

                beatmapInfo = value;

                // Refetch is scheduled, which can cause scores to be outdated if the leaderboard is not currently updating.
                // As scores are potentially used by other components, clear them eagerly to ensure a more correct state.
                SetScores(null);

                RefetchScores();
            }
        }

        private bool filterMods;

        /// <summary>
        /// Whether to apply the game's currently selected mods as a filter when retrieving scores.
        /// </summary>
        public bool FilterMods
        {
            get => filterMods;
            set
            {
                if (value == filterMods)
                    return;

                filterMods = value;

                RefetchScores();
            }
        }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? scoreSubscription;

        private GetScoresRequest? scoreRetrievalRequest;

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.ValueChanged += _ => RefetchScores();
            mods.ValueChanged += _ =>
            {
                if (filterMods)
                    RefetchScores();
            };
        }

        protected override bool IsOnlineScope => Scope != BeatmapLeaderboardScope.Local;

        protected override APIRequest? FetchScores(CancellationToken cancellationToken)
        {
            scoreRetrievalRequest?.Cancel();
            scoreRetrievalRequest = null;

            var fetchBeatmapInfo = BeatmapInfo;

            if (fetchBeatmapInfo == null)
            {
                SetErrorState(LeaderboardState.NoneSelected);
                return null;
            }

            var fetchRuleset = ruleset.Value ?? fetchBeatmapInfo.Ruleset;

            if (Scope == BeatmapLeaderboardScope.Local)
            {
                subscribeToLocalScores(fetchBeatmapInfo, cancellationToken);
                return null;
            }

            if (!api.IsLoggedIn)
            {
                SetErrorState(LeaderboardState.NotLoggedIn);
                return null;
            }

            if (!fetchRuleset.IsLegacyRuleset())
            {
                SetErrorState(LeaderboardState.RulesetUnavailable);
                return null;
            }

            if (fetchBeatmapInfo.OnlineID <= 0 || fetchBeatmapInfo.Status <= BeatmapOnlineStatus.Pending)
            {
                SetErrorState(LeaderboardState.BeatmapUnavailable);
                return null;
            }

            if (!api.LocalUser.Value.IsSupporter && (Scope != BeatmapLeaderboardScope.Global || filterMods))
            {
                SetErrorState(LeaderboardState.NotSupporter);
                return null;
            }

            IReadOnlyList<Mod>? requestMods = null;

            if (filterMods && !mods.Value.Any())
                // add nomod for the request
                requestMods = new Mod[] { new ModNoMod() };
            else if (filterMods)
                requestMods = mods.Value;

            var newRequest = new GetScoresRequest(fetchBeatmapInfo, fetchRuleset, Scope, requestMods);
            newRequest.Success += response => Schedule(() =>
            {
                // Request may have changed since fetch request.
                // Can't rely on request cancellation due to Schedule inside SetScores so let's play it safe.
                if (!newRequest.Equals(scoreRetrievalRequest))
                    return;

                SetScores(
                    response.Scores.Select(s => s.ToScoreInfo(rulesets, fetchBeatmapInfo)).OrderByTotalScore(),
                    response.UserScore?.CreateScoreInfo(rulesets, fetchBeatmapInfo)
                );
            });

            return scoreRetrievalRequest = newRequest;
        }

        protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index) => new LeaderboardScore(model, index, IsOnlineScope)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };

        protected override LeaderboardScore CreateDrawableTopScore(ScoreInfo model) => new LeaderboardScore(model, model.Position, false)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };

        private void subscribeToLocalScores(BeatmapInfo beatmapInfo, CancellationToken cancellationToken)
        {
            Debug.Assert(beatmapInfo != null);

            scoreSubscription?.Dispose();
            scoreSubscription = null;

            scoreSubscription = realm.RegisterForNotifications(r =>
                r.All<ScoreInfo>().Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $0"
                                          + $" AND {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                          + $" AND {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $1"
                                          + $" AND {nameof(ScoreInfo.DeletePending)} == false"
                    , beatmapInfo.ID, ruleset.Value.ShortName), localScoresChanged);

            void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                // This subscription may fire from changes to linked beatmaps, which we don't care about.
                // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
                if (changes?.HasCollectionChanges() == false)
                    return;

                var scores = sender.AsEnumerable();

                if (filterMods && !mods.Value.Any())
                {
                    // we need to filter out all scores that have any mods to get all local nomod scores
                    scores = scores.Where(s => !s.Mods.Any());
                }
                else if (filterMods)
                {
                    // otherwise find all the scores that have all of the currently selected mods (similar to how web applies mod filters)
                    // we're creating and using a string HashSet representation of selected mods so that it can be translated into the DB query itself
                    var selectedMods = mods.Value.Select(m => m.Acronym).ToHashSet();

                    scores = scores.Where(s => selectedMods.SetEquals(s.Mods.Select(m => m.Acronym)));
                }

                scores = scores.Detach().OrderByTotalScore();

                SetScores(scores);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            scoreSubscription?.Dispose();
            scoreRetrievalRequest?.Cancel();
        }
    }
}
