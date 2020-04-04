// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    public class TopLocalRank : UpdateableRank
    {
        private readonly BeatmapInfo beatmap;

        private ScoreManager scores;
        private IBindable<RulesetInfo> ruleset;
        private IAPIProvider api;

        protected override double LoadDelay => 250;

        public TopLocalRank(BeatmapInfo beatmap)
            : base(null)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scores, IBindable<RulesetInfo> ruleset, IAPIProvider api)
        {
            scores.ItemAdded += scoreChanged;
            scores.ItemRemoved += scoreChanged;
            ruleset.ValueChanged += _ => fetchAndLoadTopScore();

            this.ruleset = ruleset.GetBoundCopy();
            this.scores = scores;
            this.api = api;

            fetchAndLoadTopScore();
        }

        private void scoreChanged(ScoreInfo score)
        {
            if (score.BeatmapInfoID == beatmap.ID)
            {
                fetchAndLoadTopScore();
            }
        }

        private void fetchAndLoadTopScore()
        {
            var score = fetchTopScore();

            loadTopScore(score);
        }

        private void loadTopScore(ScoreInfo score)
        {
            var rank = score?.Rank;

            // toggle the display of this drawable
            // we do not want empty space if there is no rank to be displayed
            if (rank.HasValue)
                Show();
            else
                Hide();

            Schedule(() => Rank = rank);
        }

        private ScoreInfo fetchTopScore()
        {
            if (scores == null || beatmap == null || ruleset?.Value == null || api?.LocalUser.Value == null)
                return null;

            return scores.QueryScores(s => s.UserID == api.LocalUser.Value.Id && s.BeatmapInfoID == beatmap.ID && s.RulesetID == ruleset.Value.ID && !s.DeletePending)
                         .OrderByDescending(s => s.TotalScore)
                         .FirstOrDefault();
        }
    }
}
