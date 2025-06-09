// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Ranking
{
    public partial class SoloResultsScreen : ResultsScreen
    {
        private readonly IBindable<LeaderboardScores?> globalScores = new Bindable<LeaderboardScores?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private LeaderboardManager leaderboardManager { get; set; } = null!;

        public SoloResultsScreen(ScoreInfo score)
            : base(score)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            globalScores.BindTo(leaderboardManager.Scores);
        }

        protected override async Task<ScoreInfo[]> FetchScores()
        {
            Debug.Assert(Score != null);

            if (Score.BeatmapInfo!.OnlineID <= 0 || Score.BeatmapInfo.Status <= BeatmapOnlineStatus.Pending)
                return [];

            var criteria = new LeaderboardCriteria(
                Score.BeatmapInfo!,
                Score.Ruleset,
                leaderboardManager.CurrentCriteria?.Scope ?? BeatmapLeaderboardScope.Global,
                leaderboardManager.CurrentCriteria?.ExactMods
            );
            var requestTaskSource = new TaskCompletionSource<LeaderboardScores>();
            globalScores.BindValueChanged(_ =>
            {
                if (globalScores.Value != null && leaderboardManager.CurrentCriteria?.Equals(criteria) == true)
                    requestTaskSource.TrySetResult(globalScores.Value);
            });
            leaderboardManager.FetchWithCriteria(criteria, forceRefresh: true);

            var result = await requestTaskSource.Task.ConfigureAwait(false);

            if (result.FailState != null)
            {
                Logger.Log($"Failed to fetch scores (beatmap: {Score.BeatmapInfo}, ruleset: {Score.Ruleset}): {result.FailState}");
                return [];
            }

            var clonedScores = result.AllScores.Select(s => s.DeepClone()).ToArray();

            List<ScoreInfo> sortedScores = [];

            foreach (var clonedScore in clonedScores)
            {
                // ensure that we do not double up on the score being presented here.
                // additionally, ensure that the reference that ends up in `sortedScores` is the `Score` reference specifically.
                // this simplifies handling later.
                if (clonedScore.Equals(Score) || clonedScore.MatchesOnlineID(Score))
                {
                    Score.Position = clonedScore.Position;
                    sortedScores.Add(Score);
                }
                else
                {
                    bool isOnlineLeaderboard = criteria.Scope != BeatmapLeaderboardScope.Local;
                    bool presentingLocalUserScore = Score.UserID == api.LocalUser.Value.OnlineID;
                    bool presentedLocalUserScoreIsBetter = presentingLocalUserScore && clonedScore.UserID == api.LocalUser.Value.OnlineID && clonedScore.TotalScore < Score.TotalScore;

                    if (isOnlineLeaderboard && presentedLocalUserScoreIsBetter)
                        continue;

                    sortedScores.Add(clonedScore);
                }
            }

            // if we haven't encountered a match for the presented score, we still need to attach it.
            // note that the above block ensuring that the `Score` reference makes it in here makes this valid to write in this way.
            if (!sortedScores.Contains(Score))
                sortedScores.Add(Score);

            sortedScores = sortedScores.OrderByTotalScore().ToList();

            int delta = 0;
            bool isPartialLeaderboard = leaderboardManager.CurrentCriteria?.Scope != BeatmapLeaderboardScope.Local && result.TopScores.Count >= 50;

            for (int i = 0; i < sortedScores.Count; i++)
            {
                var sortedScore = sortedScores[i];

                // see `SoloGameplayLeaderboardProvider.sort()` for another place that does the same thing with slight deviations
                // if this code is changed, that code should probably be changed as well

                if (!isPartialLeaderboard)
                    sortedScore.Position = i + 1;
                else
                {
                    if (ReferenceEquals(sortedScore, Score) && sortedScore.Position == null)
                    {
                        int? previousScorePosition = i > 0 ? sortedScores[i - 1].Position : 0;
                        int? nextScorePosition = i < result.TopScores.Count - 1 ? sortedScores[i + 1].Position : null;

                        if (previousScorePosition != null && nextScorePosition != null && previousScorePosition + 1 == nextScorePosition)
                        {
                            sortedScore.Position = previousScorePosition + 1;
                            delta += 1;
                        }
                        else
                            sortedScore.Position = null;
                    }
                    else
                        sortedScore.Position += delta;
                }
            }

            // there's a non-zero chance that the `Score.Position` was mutated above,
            // but that is not actually coupled to `ScorePosition` of the relevant score panel in any way,
            // so ensure that the drawable panel also receives the updated position.
            // note that this is valid to do precisely because we ensured `Score` was in `sortedScores` earlier.
            ScorePanelList.GetPanelForScore(Score).ScorePosition.Value = Score.Position;

            sortedScores.Remove(Score);
            return sortedScores.ToArray();
        }
    }
}
