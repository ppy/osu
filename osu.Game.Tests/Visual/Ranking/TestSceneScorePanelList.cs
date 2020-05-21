// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Ranking;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanelList : OsuTestScene
    {
        private ScorePanelList list;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = list = new ScorePanelList(new TestScoreInfo(new OsuRuleset().RulesetInfo))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            Add(new Box
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Y,
                Width = 1,
                Colour = Color4.Red
            });
        });

        [Test]
        public void TestSingleScore()
        {
        }

        [Test]
        public void TestManyScores()
        {
            AddStep("add many scores", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            });
        }
    }
}
