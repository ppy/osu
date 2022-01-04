// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public class SoloResultsScreen : ResultsScreen
    {
        private GetScoresRequest getScoreRequest;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        public SoloResultsScreen(ScoreInfo score, bool allowRetry)
            : base(score, allowRetry)
        {
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            if (Score.BeatmapInfo.OnlineID == null || Score.BeatmapInfo.Status <= BeatmapOnlineStatus.Pending)
                return null;

            getScoreRequest = new GetScoresRequest(Score.BeatmapInfo, Score.Ruleset);
            getScoreRequest.Success += r => scoresCallback?.Invoke(r.Scores.Where(s => s.OnlineID != Score.OnlineID).Select(s => s.CreateScoreInfo(rulesets, Beatmap.Value.BeatmapInfo)));
            return getScoreRequest;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            getScoreRequest?.Cancel();
        }
    }
}
