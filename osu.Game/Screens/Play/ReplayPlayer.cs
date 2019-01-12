// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player
    {
        private readonly Score score;

        public ReplayPlayer(Score score)
        {
            this.score = score;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RulesetContainer.SetReplayScore(score);
        }

        protected override ScoreInfo CreateScore() => score.ScoreInfo;
    }
}
