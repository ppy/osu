// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneGradedCircles : OsuTestScene
    {
        private readonly GradedCircles ring;

        public TestSceneGradedCircles()
        {
            ScoreProcessor scoreProcessor = new OsuRuleset().CreateScoreProcessor();
            double accuracyX = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.X);
            double accuracyS = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.S);

            double accuracyA = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.A);
            double accuracyB = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.B);
            double accuracyC = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.C);

            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(400),
                Child = ring = new GradedCircles(accuracyC, accuracyB, accuracyA, accuracyS, accuracyX)
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Progress", 0.0, 1.0, 1.0, p => ring.Progress = p);
        }
    }
}
