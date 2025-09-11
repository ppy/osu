// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.RoundResults
{
    internal partial class RoundResultsScorePanel : CompositeDrawable
    {
        public RoundResultsScorePanel(ScoreInfo score)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new InstantSizingScorePanel(score);
        }

        public override bool PropagateNonPositionalInputSubTree => false;
        public override bool PropagatePositionalInputSubTree => false;

        private partial class InstantSizingScorePanel : ScorePanel
        {
            public InstantSizingScorePanel(ScoreInfo score, bool isNewLocalScore = false)
                : base(score, isNewLocalScore)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                FinishTransforms(true);
            }
        }
    }
}
