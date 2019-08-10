// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class SliderEventGenerationTest
    {
        private const double start_time = 0;
        private const double span_duration = 1000;

        [Test]
        public void TestSingleSpan()
        {
            var events = SliderEventGenerator.Generate(start_time, span_duration, 1, span_duration / 2, span_duration, 1, null).ToArray();

            Assert.That(events[0].Type, Is.EqualTo(SliderEventType.Head));
            Assert.That(events[0].Time, Is.EqualTo(start_time));

            Assert.That(events[1].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[1].Time, Is.EqualTo(span_duration / 2));

            Assert.That(events[3].Type, Is.EqualTo(SliderEventType.Tail));
            Assert.That(events[3].Time, Is.EqualTo(span_duration));
        }

        [Test]
        public void TestRepeat()
        {
            var events = SliderEventGenerator.Generate(start_time, span_duration, 1, span_duration / 2, span_duration, 2, null).ToArray();

            Assert.That(events[0].Type, Is.EqualTo(SliderEventType.Head));
            Assert.That(events[0].Time, Is.EqualTo(start_time));

            Assert.That(events[1].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[1].Time, Is.EqualTo(span_duration / 2));

            Assert.That(events[2].Type, Is.EqualTo(SliderEventType.Repeat));
            Assert.That(events[2].Time, Is.EqualTo(span_duration));

            Assert.That(events[3].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[3].Time, Is.EqualTo(span_duration + span_duration / 2));

            Assert.That(events[5].Type, Is.EqualTo(SliderEventType.Tail));
            Assert.That(events[5].Time, Is.EqualTo(2 * span_duration));
        }

        [Test]
        public void TestNonEvenTicks()
        {
            var events = SliderEventGenerator.Generate(start_time, span_duration, 1, 300, span_duration, 2, null).ToArray();

            Assert.That(events[0].Type, Is.EqualTo(SliderEventType.Head));
            Assert.That(events[0].Time, Is.EqualTo(start_time));

            Assert.That(events[1].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[1].Time, Is.EqualTo(300));

            Assert.That(events[2].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[2].Time, Is.EqualTo(600));

            Assert.That(events[3].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[3].Time, Is.EqualTo(900));

            Assert.That(events[4].Type, Is.EqualTo(SliderEventType.Repeat));
            Assert.That(events[4].Time, Is.EqualTo(span_duration));

            Assert.That(events[5].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[5].Time, Is.EqualTo(1100));

            Assert.That(events[6].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[6].Time, Is.EqualTo(1400));

            Assert.That(events[7].Type, Is.EqualTo(SliderEventType.Tick));
            Assert.That(events[7].Time, Is.EqualTo(1700));

            Assert.That(events[9].Type, Is.EqualTo(SliderEventType.Tail));
            Assert.That(events[9].Time, Is.EqualTo(2 * span_duration));
        }

        [Test]
        public void TestLegacyLastTickOffset()
        {
            var events = SliderEventGenerator.Generate(start_time, span_duration, 1, span_duration / 2, span_duration, 1, 100).ToArray();

            Assert.That(events[2].Type, Is.EqualTo(SliderEventType.LegacyLastTick));
            Assert.That(events[2].Time, Is.EqualTo(900));
        }

        [Test]
        public void TestMinimumTickDistance()
        {
            const double velocity = 5;
            const double min_distance = velocity * 10;

            var events = SliderEventGenerator.Generate(start_time, span_duration, velocity, velocity, span_duration, 2, 0).ToArray();

            Assert.Multiple(() =>
            {
                int tickIndex = -1;

                while (++tickIndex < events.Length)
                {
                    if (events[tickIndex].Type != SliderEventType.Tick)
                        continue;

                    Assert.That(events[tickIndex].Time, Is.LessThan(span_duration - min_distance).Or.GreaterThan(span_duration + min_distance));
                }
            });
        }
    }
}
