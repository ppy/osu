// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            getScoreRequest.Success += r => scoresCallback.Invoke(r.Scores.Where(s => !s.MatchesOnlineID(Score)).Select(s => s.ToScoreInfo(rulesets, Beatmap.Value.BeatmapInfo)));
            return getScoreRequest;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            getScoreRequest?.Cancel();
        }
    }
}
