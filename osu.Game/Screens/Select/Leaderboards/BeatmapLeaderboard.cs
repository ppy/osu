// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class BeatmapLeaderboard : Leaderboard<BeatmapLeaderboardScope, ScoreInfo>
    {
        public Action<ScoreInfo> ScoreSelected;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private BeatmapInfo beatmapInfo;

        public BeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (beatmapInfo == value)
                    return;

                beatmapInfo = value;
                Scores = null;

                UpdateScores();
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
        private BeatmapDifficultyCache difficultyCache { get; set; }

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

            scoreManager.ItemRemoved += scoreStoreChanged;
            scoreManager.ItemUpdated += scoreStoreChanged;
        }

        protected override void Reset()
        {
            base.Reset();
            TopScore = null;
        }

        private void scoreStoreChanged(ScoreInfo score)
        {
            if (Scope != BeatmapLeaderboardScope.Local)
                return;

            if (BeatmapInfo?.ID != score.BeatmapInfoID)
                return;

            RefreshScores();
        }

        protected override bool IsOnlineScope => Scope != BeatmapLeaderboardScope.Local;

        private CancellationTokenSource loadCancellationSource;

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            loadCancellationSource?.Cancel();
            loadCancellationSource = new CancellationTokenSource();

            var cancellationToken = loadCancellationSource.Token;

            if (BeatmapInfo == null)
            {
                PlaceholderState = PlaceholderState.NoneSelected;
                return null;
            }

            if (Scope == BeatmapLeaderboardScope.Local)
            {
                var scores = scoreManager
                    .QueryScores(s => !s.DeletePending && s.BeatmapInfo.ID == BeatmapInfo.ID && s.Ruleset.ID == ruleset.Value.ID);

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

                scoreManager.OrderByTotalScoreAsync(scores.ToArray(), cancellationToken)
                            .ContinueWith(ordered => scoresCallback?.Invoke(ordered.Result), TaskContinuationOptions.OnlyOnRanToCompletion);

                return null;
            }

            if (api?.IsLoggedIn != true)
            {
                PlaceholderState = PlaceholderState.NotLoggedIn;
                return null;
            }

            if (BeatmapInfo.OnlineID == null || BeatmapInfo?.Status <= BeatmapSetOnlineStatus.Pending)
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

            var req = new GetScoresRequest(BeatmapInfo, ruleset.Value ?? BeatmapInfo.Ruleset, Scope, requestMods);

            req.Success += r =>
            {
                scoreManager.OrderByTotalScoreAsync(r.Scores.Select(s => s.CreateScoreInfo(rulesets, BeatmapInfo)).ToArray(), cancellationToken)
                            .ContinueWith(ordered => Schedule(() =>
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    return;

                                scoresCallback?.Invoke(ordered.Result);
                                TopScore = r.UserScore?.CreateScoreInfo(rulesets, BeatmapInfo);
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

            if (scoreManager != null)
            {
                scoreManager.ItemRemoved -= scoreStoreChanged;
                scoreManager.ItemUpdated -= scoreStoreChanged;
            }
        }
    }
}
