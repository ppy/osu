// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Screens.Select.Leaderboards
{
    public partial class StateTrackingLeaderboardProvider : Component
    {
        /// <summary>
        /// List of all fetched scores.
        /// <see langword="null"/> if fetch is in progress.
        /// Updates to this bindable may not be delivered on the update thread. Consumers are expected to schedule locally as required.
        /// </summary>
        public Bindable<(IEnumerable<ScoreInfo> best, ScoreInfo? userScore)?> Scores => scores;

        private Bindable<(IEnumerable<ScoreInfo>, ScoreInfo?)?> scores { get; } = new Bindable<(IEnumerable<ScoreInfo>, ScoreInfo?)?>();

        /// <summary>
        /// Raised when fetching scores fails.
        /// This event may not be invoked on the update thread. Consumers are expected to schedule locally as required.
        /// </summary>
        public event Action<Exception>? RetrievalFailed;

        public Bindable<BeatmapLeaderboardScope> Scope { get; } = new Bindable<BeatmapLeaderboardScope>();
        public Bindable<BeatmapInfo> Beatmap { get; } = new Bindable<BeatmapInfo>();
        public Bindable<RulesetInfo> Ruleset { get; } = new Bindable<RulesetInfo>();
        public Bindable<bool> ModFilterActive { get; } = new BindableBool();
        public Bindable<IReadOnlyList<Mod>> Mods { get; } = new Bindable<IReadOnlyList<Mod>>([]);

        private readonly LeaderboardProvider leaderboardProvider;

        private IDisposable? localScoreSubscription;
        private CancellationTokenSource? onlineLookupCancellationTokenSource;

        public StateTrackingLeaderboardProvider(LeaderboardProvider leaderboardProvider)
        {
            this.leaderboardProvider = leaderboardProvider;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scope.BindValueChanged(_ => Scheduler.AddOnce(refetch));
            Beatmap.BindValueChanged(_ => Scheduler.AddOnce(refetch));
            Ruleset.BindValueChanged(_ => Scheduler.AddOnce(refetch));
            ModFilterActive.BindValueChanged(_ => Scheduler.AddOnce(refetch));
            Mods.BindValueChanged(_ => Scheduler.AddOnce(refetch));
            refetch();
        }

        private void refetch()
        {
            localScoreSubscription?.Dispose();
            localScoreSubscription = null;

            onlineLookupCancellationTokenSource?.Cancel();
            onlineLookupCancellationTokenSource = null;

            if (Scope.Value == BeatmapLeaderboardScope.Local)
            {
                localScoreSubscription = leaderboardProvider.SubscribeToLocalScores(Beatmap.Value, Ruleset.Value, localScoresChanged);
            }
            else
            {
                onlineLookupCancellationTokenSource = new CancellationTokenSource();
                scores.Value = null;
                leaderboardProvider.GetOnlineScoresAsync(Beatmap.Value, Ruleset.Value, ModFilterActive.Value ? Mods.Value : null, Scope.Value, onlineLookupCancellationTokenSource.Token)
                                   .ContinueWith(t =>
                                   {
                                       switch (t.Status)
                                       {
                                           case TaskStatus.RanToCompletion:
                                               scores.Value = t.GetResultSafely();
                                               break;

                                           case TaskStatus.Faulted:
                                               RetrievalFailed?.Invoke(t.Exception!);
                                               break;
                                       }
                                   });
            }
        }

        private void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes)
        {
            // This subscription may fire from changes to linked beatmaps, which we don't care about.
            // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
            if (changes?.HasCollectionChanges() == false)
                return;

            var newScores = sender.AsEnumerable();

            if (ModFilterActive.Value && !Mods.Value.Any())
            {
                // we need to filter out all scores that have any mods to get all local nomod scores
                newScores = newScores.Where(s => !s.Mods.Any());
            }
            else if (ModFilterActive.Value)
            {
                // otherwise find all the scores that have all of the currently selected mods (similar to how web applies mod filters)
                // we're creating and using a string HashSet representation of selected mods so that it can be translated into the DB query itself
                var selectedMods = Mods.Value.Select(m => m.Acronym).ToHashSet();

                newScores = newScores.Where(s => selectedMods.SetEquals(s.Mods.Select(m => m.Acronym)));
            }

            newScores = newScores.Detach().OrderByTotalScore();
            scores.Value = (newScores, null);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            localScoreSubscription?.Dispose();
            localScoreSubscription = null;

            onlineLookupCancellationTokenSource?.Cancel();
            onlineLookupCancellationTokenSource = null;
        }
    }
}
