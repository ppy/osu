// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Scoring;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchHealthProcessorTest
    {
        private static readonly object[][] test_cases =
        [
            // hitobject, starting HP, fail expected after miss
            [new Fruit(), 0.01, true],
            [new Droplet(), 0.01, true],
            [new TinyDroplet(), 0, false],
            [new Banana(), 0, false],
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestFailAfterMinResult(CatchHitObject hitObject, double startingHealth, bool failExpected)
        {
            var healthProcessor = new CatchHealthProcessor(0);
            healthProcessor.ApplyBeatmap(new CatchBeatmap
            {
                HitObjects = { hitObject }
            });
            healthProcessor.Health.Value = startingHealth;

            var result = new CatchJudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MinResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.EqualTo(failExpected));
        }

        [TestCaseSource(nameof(test_cases))]
        public void TestNoFailAfterMaxResult(CatchHitObject hitObject, double startingHealth, bool _)
        {
            var healthProcessor = new CatchHealthProcessor(0);
            healthProcessor.ApplyBeatmap(new CatchBeatmap
            {
                HitObjects = { hitObject }
            });
            healthProcessor.Health.Value = startingHealth;

            var result = new CatchJudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MaxResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.False);
        }
    }
}
