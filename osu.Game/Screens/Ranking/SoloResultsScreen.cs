// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Screens.Ranking
{
    public partial class SoloResultsScreen : ResultsScreen
    {
        private GetScoresRequest getScoreRequest;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private SoloStatisticsWatcher soloStatisticsWatcher { get; set; }

        private readonly Bindable<SoloStatisticsUpdate> statisticsUpdate = new Bindable<SoloStatisticsUpdate>();

        public SoloResultsScreen(ScoreInfo score, bool allowRetry)
            : base(score, allowRetry)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            soloStatisticsWatcher.RegisterForStatisticsUpdateAfter(Score, update => statisticsUpdate.Value = update);
        }

        protected override StatisticsPanel CreateStatisticsPanel() => new SoloStatisticsPanel(Score)
        {
            StatisticsUpdate = { BindTarget = statisticsUpdate }
        };

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            if (Score.BeatmapInfo.OnlineID <= 0 || Score.BeatmapInfo.Status <= BeatmapOnlineStatus.Pending)
                return null;

            getScoreRequest = new GetScoresRequest(Score.BeatmapInfo, Score.Ruleset);
            getScoreRequest.Success += r => scoresCallback?.Invoke(r.Scores.Where(s => s.OnlineID != Score.OnlineID).Select(s => s.ToScoreInfo(rulesets, Beatmap.Value.BeatmapInfo)));
            return getScoreRequest;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            getScoreRequest?.Cancel();
        }
    }
}
