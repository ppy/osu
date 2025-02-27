// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public partial class SoloResultsScreen : ResultsScreen
    {
        private GetScoresRequest? getScoreRequest;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public SoloResultsScreen(ScoreInfo score)
            : base(score)
        {
        }

        protected override APIRequest? FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            Debug.Assert(Score != null);

            if (Score.BeatmapInfo!.OnlineID <= 0 || Score.BeatmapInfo.Status <= BeatmapOnlineStatus.Pending)
                return null;

            getScoreRequest = new GetScoresRequest(Score.BeatmapInfo, Score.Ruleset);
            getScoreRequest.Success += r =>
            {
                var toDisplay = new List<ScoreInfo>();

                for (int i = 0; i < r.Scores.Count; ++i)
                {
                    var score = r.Scores[i];
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
                        var converted = score.ToScoreInfo(rulesets, Beatmap.Value.BeatmapInfo);
                        converted.Position = position;
                        toDisplay.Add(converted);
                    }
                }

                scoresCallback.Invoke(toDisplay);
            };
            return getScoreRequest;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            getScoreRequest?.Cancel();
        }
    }
}
