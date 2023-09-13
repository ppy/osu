// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneScoring : ScoringTestScene
    {
        public TestSceneScoring()
            : base(supportsNonPerfectJudgements: false)
        {
        }

        protected override IBeatmap CreateBeatmap(int maxCombo)
        {
            var beatmap = new CatchBeatmap();
            for (int i = 0; i < maxCombo; ++i)
                beatmap.HitObjects.Add(new Fruit());
            return beatmap;
        }

        protected override IScoringAlgorithm CreateScoreV1() => new ScoreV1();

        protected override IScoringAlgorithm CreateScoreV2(int maxCombo) => new ScoreV2(maxCombo);

        protected override ProcessorBasedScoringAlgorithm CreateScoreAlgorithm(IBeatmap beatmap, ScoringMode mode) => new CatchProcessorBasedScoringAlgorithm(beatmap, mode);

        private class ScoreV1 : IScoringAlgorithm
        {
            public void ApplyHit()
            {
            }

            public void ApplyNonPerfect()
            {
            }

            public void ApplyMiss()
            {
            }

            public long TotalScore => 0;
        }

        private class ScoreV2 : IScoringAlgorithm
        {
            private readonly int maxCombo;

            public ScoreV2(int maxCombo)
            {
                this.maxCombo = maxCombo;
            }

            public void ApplyHit()
            {
            }

            public void ApplyNonPerfect()
            {
            }

            public void ApplyMiss()
            {
            }

            public long TotalScore => 0;
        }

        private class CatchProcessorBasedScoringAlgorithm : ProcessorBasedScoringAlgorithm
        {
            public CatchProcessorBasedScoringAlgorithm(IBeatmap beatmap, ScoringMode mode)
                : base(beatmap, mode)
            {
            }

            protected override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor();

            protected override JudgementResult CreatePerfectJudgementResult() => new CatchJudgementResult(new Fruit(), new CatchJudgement()) { Type = HitResult.Great };

            protected override JudgementResult CreateNonPerfectJudgementResult() => throw new NotSupportedException("catch does not have \"non-perfect\" judgements.");

            protected override JudgementResult CreateMissJudgementResult() => new CatchJudgementResult(new Fruit(), new CatchJudgement()) { Type = HitResult.Miss };
        }
    }
}
