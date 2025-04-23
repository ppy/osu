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

        private readonly Bindable<LeaderboardScores?> fetchedScores = new Bindable<LeaderboardScores?>();

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
            ((IBindable<LeaderboardScores?>)fetchedScores).BindTo(leaderboardManager.Scores);
        }

        protected override bool IsOnlineScope => Scope != BeatmapLeaderboardScope.Local;

        protected override APIRequest? FetchScores(CancellationToken cancellationToken)
        {
            var fetchBeatmapInfo = BeatmapInfo;
            var fetchRuleset = ruleset.Value ?? fetchBeatmapInfo?.Ruleset;

            leaderboardManager.FetchWithCriteriaAsync(new LeaderboardCriteria(fetchBeatmapInfo, fetchRuleset, Scope, filterMods ? mods.Value.ToArray() : null))
                              .ContinueWith(t =>
                              {
                                  if (t.Exception != null && !t.IsCanceled)
                                  {
                                      Schedule(() => SetErrorState(LeaderboardState.NetworkFailure));
                                      return;
                                  }

                                  fetchedScores.UnbindEvents();
                                  fetchedScores.BindValueChanged(scores =>
                                  {
                                      if (scores.NewValue == null) return;

                                      if (scores.NewValue.FailState == null)
                                          Schedule(() => SetScores(scores.NewValue.TopScores, scores.NewValue.UserScore));
                                      else
                                          Schedule(() => SetErrorState((LeaderboardState)scores.NewValue.FailState));
                                  }, true);
                              }, cancellationToken);

            return null;
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
