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
        [Resolved]
        private RulesetStore rulesets { get; set; }

        public SoloResultsScreen(ScoreInfo score, bool allowRetry)
            : base(score, allowRetry)
        {
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            if (Score.Beatmap.OnlineBeatmapID == null || Score.Beatmap.Status <= BeatmapSetOnlineStatus.Pending)
                return null;

            var req = new GetScoresRequest(Score.Beatmap, Score.Ruleset);
            req.Success += r => scoresCallback?.Invoke(r.Scores.Where(s => s.OnlineScoreID != Score.OnlineScoreID).Select(s => s.CreateScoreInfo(rulesets)));
            return req;
        }
    }
}
