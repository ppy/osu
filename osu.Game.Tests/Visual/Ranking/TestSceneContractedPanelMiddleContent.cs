// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Contracted;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneContractedPanelMiddleContent : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Test]
        public void TestShowPanel()
        {
            AddStep("show example score", () => showPanel(CreateWorkingBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo)), new TestScoreInfo(new OsuRuleset().RulesetInfo)));
        }

        private void showPanel(WorkingBeatmap workingBeatmap, ScoreInfo score)
        {
            Child = new ContractedPanelMiddleContentContainer(workingBeatmap, score);
        }

        private class ContractedPanelMiddleContentContainer : Container
        {
            [Cached]
            private Bindable<WorkingBeatmap> workingBeatmap { get; set; }

            public ContractedPanelMiddleContentContainer(WorkingBeatmap beatmap, ScoreInfo score)
            {
                workingBeatmap = new Bindable<WorkingBeatmap>(beatmap);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(ScorePanel.CONTRACTED_WIDTH, 460);
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#353535"),
                    },
                    new ContractedPanelMiddleContent(score),
                };
            }
        }
    }
}
