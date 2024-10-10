// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuHealthProcessorTest
    {
        private static readonly object[][] test_cases =
        [
            // hitobject, starting HP, fail expected after miss
            [new HitCircle(), 0.01, true],
            [new SliderHeadCircle(), 0.01, true],
            [new SliderHeadCircle { ClassicSliderBehaviour = true }, 0.01, true],
            [new SliderTick(), 0.01, true],
            [new SliderRepeat(new Slider()), 0.01, true],
            [new SliderTailCircle(new Slider()), 0, true],
            [new SliderTailCircle(new Slider()) { ClassicSliderBehaviour = true }, 0.01, true],
            [new Slider(), 0, true],
            [new Slider { ClassicSliderBehaviour = true }, 0.01, true],
            [new SpinnerTick(), 0, false],
            [new SpinnerBonusTick(), 0, false],
            [new Spinner(), 0.01, true],
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestFailAfterMinResult(OsuHitObject hitObject, double startingHealth, bool failExpected)
        {
            var healthProcessor = new OsuHealthProcessor(0);
            healthProcessor.ApplyBeatmap(new OsuBeatmap
            {
                HitObjects = { hitObject }
            });
            healthProcessor.Health.Value = startingHealth;

            var result = new OsuJudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MinResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.EqualTo(failExpected));
        }

        [TestCaseSource(nameof(test_cases))]
        public void TestNoFailAfterMaxResult(OsuHitObject hitObject, double startingHealth, bool _)
        {
            var healthProcessor = new OsuHealthProcessor(0);
            healthProcessor.ApplyBeatmap(new OsuBeatmap
            {
                HitObjects = { hitObject }
            });
            healthProcessor.Health.Value = startingHealth;

            var result = new OsuJudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MaxResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.False);
        }
    }
}
