// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneCatcherTrail : OsuTestScene
    {
        [Test]
        public void TestCatcherTrailClock()
        {
            TestCatcherTrail trail = null!;

            AddStep("create trail", () =>
            {
                trail = new TestCatcherTrail();
            });

            AddStep("apply entry at time 1000", () =>
            {
                var entry = new CatcherTrailEntry(1000, CatcherAnimationState.Idle, 0, Vector2.One, CatcherTrailAnimation.Dashing);
                trail.Apply(entry);
            });

            AddAssert("clock is at 1000", () => trail.GetInnerClockTime() == 1000);
        }

        private partial class TestCatcherTrail : CatcherTrail
        {
            public double GetInnerClockTime()
            {
                // InternalChildren[0] is the body (SkinnableCatcher)
                var body = (CompositeDrawable)InternalChildren[0];
                return body.Clock.CurrentTime;
            }
        }
    }
}
