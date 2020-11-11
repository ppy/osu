// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player
    {
        protected readonly Score Score;

        // Disallow replays from failing. (see https://github.com/ppy/osu/issues/6108)
        protected override bool CheckModsAllowFailure() => false;

        public ReplayPlayer(Score score, bool allowPause = true, bool showResults = true)
            : base(allowPause, showResults)
        {
            Score = score;
        }

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(Score);
        }

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score, false);

        protected override ScoreInfo CreateScore()
        {
            var baseScore = base.CreateScore();

            // Since the replay score doesn't contain statistics, we'll pass them through here.
            Score.ScoreInfo.HitEvents = baseScore.HitEvents;

            return Score.ScoreInfo;
        }
    }
}
