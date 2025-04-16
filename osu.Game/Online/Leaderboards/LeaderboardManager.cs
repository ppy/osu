// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using Realms;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardManager : Component
    {
        public IBindable<LeaderboardScores?> Scores => scores;
        private readonly Bindable<LeaderboardScores?> scores = new Bindable<LeaderboardScores?>();

        public LeaderboardCriteria? CurrentCriteria { get; private set; }

        private IDisposable? localScoreSubscription;
        private TaskCompletionSource<LeaderboardScores?>? localFetchCompletionSource;
        private TaskCompletionSource<LeaderboardScores?>? lastFetchCompletionSource;
        private GetScoresRequest? inFlightOnlineRequest;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public Task<LeaderboardScores?> FetchWithCriteriaAsync(LeaderboardCriteria newCriteria)
        {
            if (CurrentCriteria?.Equals(newCriteria) == true && lastFetchCompletionSource?.Task.IsFaulted == false)
                return lastFetchCompletionSource?.Task ?? Task.FromResult(Scores.Value);

            CurrentCriteria = newCriteria;
            localScoreSubscription?.Dispose();
            inFlightOnlineRequest?.Cancel();
            lastFetchCompletionSource?.TrySetCanceled();
            scores.Value = null;

            switch (newCriteria.Scope)
            {
                case BeatmapLeaderboardScope.Local:
                {
                    // this task completion source will be marked completed in the `localScoresChanged()` below.
                    // yes it's twisty, but such are the costs of trying to reconcile data-push / subscription and data-pull / explicit fetch flows.
                    lastFetchCompletionSource = localFetchCompletionSource = new TaskCompletionSource<LeaderboardScores?>();
                    localScoreSubscription = realm.RegisterForNotifications(r =>
                        r.All<ScoreInfo>().Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $0"
                                                  + $" AND {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                                  + $" AND {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $1"
                                                  + $" AND {nameof(ScoreInfo.DeletePending)} == false"
                            , newCriteria.Beatmap.ID, newCriteria.Ruleset.ShortName), localScoresChanged);
                    return localFetchCompletionSource.Task;
                }

                default:
                {
                    var onlineFetchCompletionSource = new TaskCompletionSource<LeaderboardScores?>();
                    lastFetchCompletionSource = onlineFetchCompletionSource;

                    IReadOnlyList<Mod>? requestMods = null;

                    if (newCriteria.ExactMods != null)
                    {
                        if (!newCriteria.ExactMods.Any())
                            // add nomod for the request
                            requestMods = new Mod[] { new ModNoMod() };
                        else
                            requestMods = newCriteria.ExactMods;
                    }

                    var newRequest = new GetScoresRequest(newCriteria.Beatmap, newCriteria.Ruleset, newCriteria.Scope, requestMods);
                    newRequest.Success += response =>
                    {
                        if (inFlightOnlineRequest != null && !newRequest.Equals(inFlightOnlineRequest))
                            return;

                        var result = new LeaderboardScores
                        (
                            response.Scores.Select(s => s.ToScoreInfo(rulesets, newCriteria.Beatmap)).OrderByTotalScore().ToArray(),
                            response.UserScore?.CreateScoreInfo(rulesets, newCriteria.Beatmap)
                        );
                        inFlightOnlineRequest = null;
                        if (onlineFetchCompletionSource.TrySetResult(result))
                            scores.Value = result;
                    };
                    newRequest.Failure += ex => onlineFetchCompletionSource.TrySetException(ex);
                    api.Queue(inFlightOnlineRequest = newRequest);
                    return onlineFetchCompletionSource.Task;
                }
            }
        }

        private void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes)
        {
            Debug.Assert(CurrentCriteria != null);

            // This subscription may fire from changes to linked beatmaps, which we don't care about.
            // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
            if (changes?.HasCollectionChanges() == false)
                return;

            var newScores = sender.AsEnumerable();

            if (CurrentCriteria.ExactMods != null)
            {
                if (!CurrentCriteria.ExactMods.Any())
                {
                    // we need to filter out all scores that have any mods to get all local nomod scores
                    newScores = newScores.Where(s => !s.Mods.Any());
                }
                else
                {
                    // otherwise find all the scores that have all of the currently selected mods (similar to how web applies mod filters)
                    // we're creating and using a string HashSet representation of selected mods so that it can be translated into the DB query itself
                    var selectedMods = CurrentCriteria.ExactMods.Select(m => m.Acronym).ToHashSet();

                    newScores = newScores.Where(s => selectedMods.SetEquals(s.Mods.Select(m => m.Acronym)));
                }
            }

            newScores = newScores.Detach().OrderByTotalScore();

            scores.Value = new LeaderboardScores(newScores.ToArray(), null);

            if (localFetchCompletionSource != null && localFetchCompletionSource == lastFetchCompletionSource)
            {
                localFetchCompletionSource.SetResult(scores.Value);
                localFetchCompletionSource = lastFetchCompletionSource = null;
            }
        }
    }

    public record LeaderboardCriteria(
        BeatmapInfo Beatmap,
        RulesetInfo Ruleset,
        BeatmapLeaderboardScope Scope,
        Mod[]? ExactMods
    );

    public record LeaderboardScores(ICollection<ScoreInfo> TopScores, ScoreInfo? UserScore)
    {
        public IEnumerable<ScoreInfo> AllScores
        {
            get
            {
                foreach (var score in TopScores)
                    yield return score;

                if (UserScore != null && TopScores.All(topScore => !topScore.Equals(UserScore) && !topScore.MatchesOnlineID(UserScore)))
                    yield return UserScore;
            }
        }
    }
}
