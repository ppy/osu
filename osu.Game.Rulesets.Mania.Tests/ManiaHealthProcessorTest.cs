// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaHealthProcessorTest
    {
        [Test]
        public void TestNoDrain()
        {
            var processor = new ManiaHealthProcessor(0);
            processor.ApplyBeatmap(new ManiaBeatmap(new StageDefinition(4))
            {
                HitObjects =
                {
                    new Note { StartTime = 0 },
                    new Note { StartTime = 1000 },
                }
            });

            // No matter what, mania doesn't have passive HP drain.
            Assert.That(processor.DrainRate, Is.Zero);
        }
    }
}
