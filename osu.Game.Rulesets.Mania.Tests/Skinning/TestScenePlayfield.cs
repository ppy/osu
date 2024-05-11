// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public partial class TestScenePlayfield : ManiaSkinnableTestScene
    {
        private List<StageDefinition> stageDefinitions = new List<StageDefinition>();

        [Test]
        public void TestSingleStage()
        {
            AddStep("create stage", () =>
            {
                stageDefinitions = new List<StageDefinition>
                {
                    new StageDefinition(2)
                };

                SetContents(_ => new ManiaInputManager(new ManiaRuleset().RulesetInfo, 2)
                {
                    Child = new ManiaPlayfield(stageDefinitions)
                });
            });
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public void TestDualStages(int columnCount)
        {
            AddStep("create stage", () =>
            {
                stageDefinitions = new List<StageDefinition>
                {
                    new StageDefinition(columnCount),
                    new StageDefinition(columnCount)
                };

                SetContents(_ => new ManiaInputManager(new ManiaRuleset().RulesetInfo, (int)PlayfieldType.Dual + 2 * columnCount)
                {
                    Child = new ManiaPlayfield(stageDefinitions)
                    {
                        // bit of a hack to make sure the dual stages fit on screen without overlapping each other.
                        Size = new Vector2(1.5f),
                        Scale = new Vector2(1 / 1.5f)
                    }
                });
            });
        }

        protected override IBeatmap CreateBeatmapForSkinProvider()
        {
            var maniaBeatmap = (ManiaBeatmap)base.CreateBeatmapForSkinProvider();
            maniaBeatmap.Stages = stageDefinitions;
            return maniaBeatmap;
        }
    }
}
