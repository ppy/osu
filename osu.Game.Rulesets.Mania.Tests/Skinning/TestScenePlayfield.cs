// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestScenePlayfield : ManiaSkinnableTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new ManiaRuleset());

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

                SetContents(_ => new ManiaPlayfield(stageDefinitions));
            });

            AddRepeatStep("perform hit", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Perfect }), 20);
            AddStep("perform miss", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Miss }));
        }

        [Test]
        public void TestDualStages()
        {
            AddStep("create stage", () =>
            {
                stageDefinitions = new List<StageDefinition>
                {
                    new StageDefinition(2),
                    new StageDefinition(2)
                };

                SetContents(_ => new ManiaPlayfield(stageDefinitions));
            });

            AddRepeatStep("perform hit", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Perfect }), 20);
            AddStep("perform miss", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Miss }));
        }

        protected override IBeatmap CreateBeatmapForSkinProvider()
        {
            var maniaBeatmap = (ManiaBeatmap)base.CreateBeatmapForSkinProvider();
            maniaBeatmap.Stages = stageDefinitions;
            return maniaBeatmap;
        }
    }
}
