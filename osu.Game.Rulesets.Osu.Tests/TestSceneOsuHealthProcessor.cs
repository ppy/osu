// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneOsuHealthProcessor
    {
        [Test]
        public void TestNoBreak()
        {
            OsuLegacyHealthProcessor hp = new OsuLegacyHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<OsuHitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 2000 }
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(1.4E-5).Within(0.1E-5));
        }

        [Test]
        public void TestSingleBreak()
        {
            OsuLegacyHealthProcessor hp = new OsuLegacyHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<OsuHitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 2000 }
                },
                Breaks =
                {
                    new BreakPeriod(500, 1500)
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(4.3E-5).Within(0.1E-5));
        }

        [Test]
        public void TestOverlappingBreak()
        {
            OsuLegacyHealthProcessor hp = new OsuLegacyHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<OsuHitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 2000 }
                },
                Breaks =
                {
                    new BreakPeriod(500, 1400),
                    new BreakPeriod(750, 1500),
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(4.3E-5).Within(0.1E-5));
        }

        [Test]
        public void TestSequentialBreak()
        {
            OsuLegacyHealthProcessor hp = new OsuLegacyHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<OsuHitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 2000 }
                },
                Breaks =
                {
                    new BreakPeriod(500, 1000),
                    new BreakPeriod(1000, 1500),
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(4.3E-5).Within(0.1E-5));
        }
    }
}
