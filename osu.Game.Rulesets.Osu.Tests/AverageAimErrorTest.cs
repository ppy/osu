// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class AverageAimErrorTest
    {
        [Test]
        public void TestAdjustedAngle()
        {
            // 4 notes in a square pattern, each hit 1 unit from the center closer to the previous note.
            var events = new[]
            {
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle { Position = new Vector2(-1, -1) }, new HitCircle { Position = new Vector2(1, -1) }, new Vector2(0, -1)),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle { Position = new Vector2(-1, 1) }, new HitCircle { Position = new Vector2(-1, -1) }, new Vector2(-1, 0)),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle { Position = new Vector2(1, 1) }, new HitCircle { Position = new Vector2(-1, 1) }, new Vector2(0, 1)),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle { Position = new Vector2(1, -1) }, new HitCircle { Position = new Vector2(1, 1) }, new Vector2(1, 0))
            };

            var aimError = new AverageAimError(events);

            Assert.AreEqual(1, aimError.Value!.Value);
        }

        [Test]
        public void TestNullPositionsReturnNull()
        {
            var events = new[]
            {
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), new HitCircle(), null),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), new HitCircle(), null),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), new HitCircle(), null),
            };

            var aimError = new AverageAimError(events);

            Assert.IsNull(aimError.Value);
        }
    }
}
