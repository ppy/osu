// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class BeatmapLeaderboard : Leaderboard<BeatmapLeaderboardScope, ScoreInfo>
    {
        public Action<ScoreInfo> ScoreSelected;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        private BeatmapInfo beatmapInfo;

        public BeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (beatmapInfo?.Equals(value) == true)
                    return;

                beatmapInfo = value;
                Scores = null;

                if (IsOnlineScope)
                    UpdateScores();
                else
                {
                    if (IsLoaded)
                        refreshRealmSubscription();
                }
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

                UpdateScores();
            }
        }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.ValueChanged += _ => UpdateScores();
            mods.ValueChanged += _ =>
            {
                if (filterMods)
                    UpdateScores();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            refreshRealmSubscription();
        }

        private IDisposable scoreSubscription;

        private void refreshRealmSubscription()
        {
            scoreSubscription?.Dispose();
            scoreSubscription = null;

            if (beatmapInfo == null)
                return;

            scoreSubscription = realm.RegisterForNotifications(r =>
                    r.All<ScoreInfo>()
                     .Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} = $0", beatmapInfo.ID),
                (_, changes, ___) =>
                {
                    if (!IsOnlineScope)
                        RefreshScores();
                });
        }

        protected override void Reset()
        {
            base.Reset();
            TopScore = null;
        }

        protected override bool IsOnlineScope => Scope != BeatmapLeaderboardScope.Local;

        private CancellationTokenSource loadCancellationSource;

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            loadCancellationSource?.Cancel();
            loadCancellationSource = new CancellationTokenSource();

            var cancellationToken = loadCancellationSource.Token;

            var fetchBeatmapInfo = BeatmapInfo;

            if (fetchBeatmapInfo == null)
            {
                PlaceholderState = PlaceholderState.NoneSelected;
                return null;
            }

            if (Scope == BeatmapLeaderboardScope.Local)
            {
                realm.Run(r =>
                {
                    var scores = r.All<ScoreInfo>()
                                  .AsEnumerable()
                                  // TODO: update to use a realm filter directly (or at least figure out the beatmap part to reduce scope).
                                  .Where(s => !s.DeletePending && s.BeatmapInfo.ID == fetchBeatmapInfo.ID && s.Ruleset.ShortName == ruleset.Value.ShortName);

                    if (filterMods && !mods.Value.Any())
                    {
                        // we need to filter out all scores that have any mods to get all local nomod scores
                        scores = scores.Where(s => !s.Mods.Any());
                    }
                    else if (filterMods)
                    {
                        // otherwise find all the scores that have *any* of the currently selected mods (similar to how web applies mod filters)
                        // we're creating and using a string list representation of selected mods so that it can be translated into the DB query itself
                        var selectedMods = mods.Value.Select(m => m.Acronym);
                        scores = scores.Where(s => s.Mods.Any(m => selectedMods.Contains(m.Acronym)));
                    }

                    scores = scores.Detach();

                    scoreManager.OrderByTotalScoreAsync(scores.ToArray(), cancellationToken)
                                .ContinueWith(ordered => scoresCallback?.Invoke(ordered.GetResultSafely()), TaskContinuationOptions.OnlyOnRanToCompletion);
                });

                return null;
            }

            if (api?.IsLoggedIn != true)
            {
                PlaceholderState = PlaceholderState.NotLoggedIn;
                return null;
            }

            if (fetchBeatmapInfo.OnlineID <= 0 || fetchBeatmapInfo.Status <= BeatmapOnlineStatus.Pending)
            {
                PlaceholderState = PlaceholderState.Unavailable;
                return null;
            }

            if (!api.LocalUser.Value.IsSupporter && (Scope != BeatmapLeaderboardScope.Global || filterMods))
            {
                PlaceholderState = PlaceholderState.NotSupporter;
                return null;
            }

            IReadOnlyList<Mod> requestMods = null;

            if (filterMods && !mods.Value.Any())
                // add nomod for the request
                requestMods = new Mod[] { new ModNoMod() };
            else if (filterMods)
                requestMods = mods.Value;

            var req = new GetScoresRequest(fetchBeatmapInfo, ruleset.Value ?? fetchBeatmapInfo.Ruleset, Scope, requestMods);

            req.Success += r =>
            {
                scoreManager.OrderByTotalScoreAsync(r.Scores.Select(s => s.CreateScoreInfo(rulesets, fetchBeatmapInfo)).ToArray(), cancellationToken)
                            .ContinueWith(task => Schedule(() =>
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    return;

                                scoresCallback?.Invoke(task.GetResultSafely());
                                TopScore = r.UserScore?.CreateScoreInfo(rulesets, fetchBeatmapInfo);
                            }), TaskContinuationOptions.OnlyOnRanToCompletion);
            };

            return req;
        }

        protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index) => new LeaderboardScore(model, index, IsOnlineScope)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };

        protected override LeaderboardScore CreateDrawableTopScore(ScoreInfo model) => new LeaderboardScore(model, model.Position, false)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            scoreSubscription?.Dispose();
        }
    }
}
