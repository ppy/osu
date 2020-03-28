// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    public class TopLocalRank : Container
    {
        private readonly BeatmapInfo beatmap;
        private readonly UpdateableRank rank;

        private ScoreManager scores;
        private IBindable<RulesetInfo> ruleset;
        private IAPIProvider api;

        /// <summary>
        /// Raised when the top score is loaded
        /// </summary>
        public Action<ScoreInfo> ScoreLoaded;

        public TopLocalRank(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;

            RelativeSizeAxes = Axes.Both;

            InternalChild = rank = new UpdateableRank(null)
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scores, IBindable<RulesetInfo> ruleset, IAPIProvider api)
        {
            this.scores = scores;
            this.ruleset = ruleset;
            this.api = api;

            FetchAndLoadTopScore();
        }

        public void FetchAndLoadTopScore()
        {
            var score = fetchTopScore();

            loadTopScore(score);
        }

        private void loadTopScore(ScoreInfo score)
        {
            Schedule(() => rank.Rank = score?.Rank);

            ScoreLoaded?.Invoke(score);
        }

        private ScoreInfo fetchTopScore()
        {
            if (scores == null || beatmap == null || ruleset?.Value == null || api?.LocalUser.Value == null)
                return null;

            return scores.GetAllUsableScores()
                         .Where(s => s.UserID == api.LocalUser.Value.Id && s.BeatmapInfoID == beatmap.ID && s.RulesetID == ruleset.Value.ID)
                         .OrderByDescending(s => s.TotalScore)
                         .FirstOrDefault();
        }
    }
}
