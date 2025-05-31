// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
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
        private GetScoresRequest? inFlightOnlineRequest;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        /// <summary>
        /// Fetch leaderboard content with the new criteria specified in the background.
        /// On completion, <see cref="Scores"/> will be updated with the results from this call (unless a more recent call with a different criteria has completed).
        /// </summary>
        public void FetchWithCriteria(LeaderboardCriteria newCriteria, bool forceRefresh = false)
        {
            if (!forceRefresh && CurrentCriteria?.Equals(newCriteria) == true && scores.Value?.FailState == null)
                return;

            CurrentCriteria = newCriteria;
            localScoreSubscription?.Dispose();
            inFlightOnlineRequest?.Cancel();
            scores.Value = null;

            if (newCriteria.Beatmap == null || newCriteria.Ruleset == null)
            {
                scores.Value = LeaderboardScores.Failure(LeaderboardFailState.NoneSelected);
                return;
            }

            switch (newCriteria.Scope)
            {
                case BeatmapLeaderboardScope.Local:
                {
                    localScoreSubscription = realm.RegisterForNotifications(r =>
                        r.All<ScoreInfo>().Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $0"
                                                  + $" AND {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                                  + $" AND {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $1"
                                                  + $" AND {nameof(ScoreInfo.DeletePending)} == false"
                            , newCriteria.Beatmap.ID, newCriteria.Ruleset.ShortName), localScoresChanged);
                    return;
                }

                default:
                {
                    if (!api.IsLoggedIn)
                    {
                        scores.Value = LeaderboardScores.Failure(LeaderboardFailState.NotLoggedIn);
                        return;
                    }

                    if (!newCriteria.Ruleset.IsLegacyRuleset())
                    {
                        scores.Value = LeaderboardScores.Failure(LeaderboardFailState.RulesetUnavailable);
                        return;
                    }

                    if (newCriteria.Beatmap.OnlineID <= 0 || newCriteria.Beatmap.Status <= BeatmapOnlineStatus.Pending)
                    {
                        scores.Value = LeaderboardScores.Failure(LeaderboardFailState.BeatmapUnavailable);
                        return;
                    }

                    if ((newCriteria.Scope.RequiresSupporter(newCriteria.ExactMods != null)) && !api.LocalUser.Value.IsSupporter)
                    {
                        scores.Value = LeaderboardScores.Failure(LeaderboardFailState.NotSupporter);
                        return;
                    }

                    if (newCriteria.Scope == BeatmapLeaderboardScope.Team && api.LocalUser.Value.Team == null)
                    {
                        scores.Value = LeaderboardScores.Failure(LeaderboardFailState.NoTeam);
                        return;
                    }

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

                        var result = LeaderboardScores.Success
                        (
                            response.Scores.Select(s => s.ToScoreInfo(rulesets, newCriteria.Beatmap))
                                    .OrderByTotalScore()
                                    .Select((s, idx) =>
                                    {
                                        s.Position = idx + 1;
                                        return s;
                                    })
                                    .ToArray(),
                            response.ScoresCount,
                            response.UserScore?.CreateScoreInfo(rulesets, newCriteria.Beatmap)
                        );
                        inFlightOnlineRequest = null;
                        scores.Value = result;
                    };
                    newRequest.Failure += ex =>
                    {
                        Logger.Log($@"Failed to fetch leaderboards when displaying results: {ex}", LoggingTarget.Network);
                        if (ex is not OperationCanceledException)
                            scores.Value = LeaderboardScores.Failure(LeaderboardFailState.NetworkFailure);
                    };

                    api.Queue(inFlightOnlineRequest = newRequest);
                    break;
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

            var newScoresArray = newScores.ToArray();
            scores.Value = LeaderboardScores.Success(newScoresArray, newScoresArray.Length, null);
        }
    }

    public record LeaderboardCriteria(
        BeatmapInfo? Beatmap,
        RulesetInfo? Ruleset,
        BeatmapLeaderboardScope Scope,
        Mod[]? ExactMods
    );

    public record LeaderboardScores
    {
        public ICollection<ScoreInfo> TopScores { get; }
        public int TotalScores { get; }
        public ScoreInfo? UserScore { get; }
        public LeaderboardFailState? FailState { get; }

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

        private LeaderboardScores(ICollection<ScoreInfo> topScores, int totalScores, ScoreInfo? userScore, LeaderboardFailState? failState)
        {
            TopScores = topScores;
            TotalScores = totalScores;
            UserScore = userScore;
            FailState = failState;
        }

        public static LeaderboardScores Success(ICollection<ScoreInfo> topScores, int totalScores, ScoreInfo? userScore) => new LeaderboardScores(topScores, totalScores, userScore, null);
        public static LeaderboardScores Failure(LeaderboardFailState failState) => new LeaderboardScores([], 0, null, failState);
    }

    public enum LeaderboardFailState
    {
        NetworkFailure = -1,
        BeatmapUnavailable = -2,
        RulesetUnavailable = -3,
        NoneSelected = -4,
        NotLoggedIn = -5,
        NotSupporter = -6,
        NoTeam = -7
    }
}
