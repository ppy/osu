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
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Ranking
{
    public partial class SoloResultsScreen : ResultsScreen
    {
        private readonly IBindable<LeaderboardScores?> globalScores = new Bindable<LeaderboardScores?>();

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

            var toDisplay = new List<ScoreInfo>();

            var scores = result.AllScores.Select(s => s.DeepClone()).ToList();

            for (int i = 0; i < scores.Count; ++i)
            {
                var score = scores[i];
                int position = i + 1;

                if (score.MatchesOnlineID(Score))
                {
                    // we don't want to add the same score twice, but also setting any properties of `Score` this late will have no visible effect,
                    // so we have to fish out the actual drawable panel and set the position to it directly.
                    var panel = ScorePanelList.GetPanelForScore(Score);
                    Score.Position = panel.ScorePosition.Value = position;
                }
                else
                {
                    score.Position = position;
                    toDisplay.Add(score);
                }
            }

            return toDisplay.ToArray();
        }
    }
}
