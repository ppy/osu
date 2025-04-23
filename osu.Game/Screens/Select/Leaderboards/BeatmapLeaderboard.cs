// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

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

        private readonly IBindable<LeaderboardScores?> fetchedScores = new Bindable<LeaderboardScores?>();

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private LeaderboardManager leaderboardManager { get; set; } = null!;

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

        private bool initialFetchComplete;

        protected override bool IsOnlineScope => Scope != BeatmapLeaderboardScope.Local;

        protected override APIRequest? FetchScores(CancellationToken cancellationToken)
        {
            var fetchBeatmapInfo = BeatmapInfo;
            var fetchRuleset = ruleset.Value ?? fetchBeatmapInfo?.Ruleset;

            // Without this check, an initial fetch will be performed and clear global cache.
            if (fetchBeatmapInfo == null)
                return null;

            // For now, we forcefully refresh to keep things simple.
            // In the future, removing this requirement may be deemed useful, but will need ample testing of edge case scenarios
            // (like returning from gameplay after setting a new score, returning to song select after main menu).
            leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(fetchBeatmapInfo, fetchRuleset, Scope, filterMods ? mods.Value.ToArray() : null), forceRefresh: true);

            if (!initialFetchComplete)
            {
                // only bind this after the first fetch to avoid reading stale scores.
                fetchedScores.BindTo(leaderboardManager.Scores);
                fetchedScores.BindValueChanged(_ => updateScores(), true);
                initialFetchComplete = true;
            }

            return null;
        }

        private void updateScores()
        {
            var scores = fetchedScores.Value;

            if (scores == null) return;

            if (scores.FailState == null)
                Schedule(() => SetScores(scores.TopScores, scores.UserScore));
            else
                Schedule(() => SetErrorState((LeaderboardState)scores.FailState));
        }

        protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index) => new LeaderboardScore(model, index, IsOnlineScope, Scope != BeatmapLeaderboardScope.Friend)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };

        protected override LeaderboardScore CreateDrawableTopScore(ScoreInfo model) => new LeaderboardScore(model, model.Position, false, Scope != BeatmapLeaderboardScope.Friend)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };
    }
}
