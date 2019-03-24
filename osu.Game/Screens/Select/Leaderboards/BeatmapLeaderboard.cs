// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class BeatmapLeaderboard : Leaderboard<BeatmapLeaderboardScope, ScoreInfo>
    {
        public Action<ScoreInfo> ScoreSelected;

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                Scores = null;

                UpdateScores();
            }
        }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.ValueChanged += _ => UpdateScores();
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            if (Scope == BeatmapLeaderboardScope.Local)
            {
                Scores = scoreManager.QueryScores(s => !s.DeletePending && s.Beatmap.ID == Beatmap.ID).ToArray();
                PlaceholderState = Scores.Any() ? PlaceholderState.Successful : PlaceholderState.NoScores;
                return null;
            }

            if (Beatmap?.OnlineBeatmapID == null)
            {
                PlaceholderState = PlaceholderState.Unavailable;
                return null;
            }

            if (Scope != BeatmapLeaderboardScope.Global && !api.LocalUser.Value.IsSupporter)
            {
                PlaceholderState = PlaceholderState.NotSupporter;
                return null;
            }

            var req = new GetScoresRequest(Beatmap, ruleset.Value ?? Beatmap.Ruleset, Scope);

            req.Success += r => scoresCallback?.Invoke(r.Scores);

            return req;
        }

        protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index) => new LeaderboardScore(model, index)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };
    }
}
