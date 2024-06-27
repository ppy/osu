using NUnit.Framework;
//using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Catch.Replays;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCatchReplayFrameFromLegacy
    {
        [Test]
        public void TestFromLegacyDashing()
        {
            var legacyFrame = new LegacyReplayFrame(0, new Vector2(10, 0).X, null, ReplayButtonState.Left1);
            var catchFrame = new CatchReplayFrame();
            catchFrame.FromLegacy(legacyFrame, null, null);

            Assert.AreEqual(10, catchFrame.Position, "TestFromLegacyDashing failed: Position mismatch");
            Assert.IsTrue(catchFrame.Dashing, "TestFromLegacyDashing failed: Dashing mismatch");
            Assert.Contains(CatchAction.Dash, catchFrame.Actions, "TestFromLegacyDashing failed: Action mismatch");
        }

        [Test]
        public void TestFromLegacyNoDashing()
        {
            var legacyFrame = new LegacyReplayFrame(0, new Vector2(10, 0).X, null, ReplayButtonState.None);
            var catchFrame = new CatchReplayFrame();
            catchFrame.FromLegacy(legacyFrame, null, null);

            Assert.AreEqual(10, catchFrame.Position, "TestFromLegacyNoDashing failed: Position mismatch");
            Assert.IsFalse(catchFrame.Dashing, "TestFromLegacyNoDashing failed: Dashing mismatch");
            Assert.IsFalse(catchFrame.Actions.Contains(CatchAction.Dash), "TestFromLegacyNoDashing failed: Action mismatch");
        }

        [Test]
        public void TestFromLegacyMoveRight()
        {
            var legacyFrame = new LegacyReplayFrame(0, new Vector2(20, 0).X, null, ReplayButtonState.None);
            var lastFrame = new CatchReplayFrame(0, 10, false);
            var catchFrame = new CatchReplayFrame();
            catchFrame.FromLegacy(legacyFrame, null, lastFrame);

            Assert.AreEqual(20, catchFrame.Position, "TestFromLegacyMoveRight failed: Position mismatch");
            Assert.IsTrue(lastFrame.Actions.Contains(CatchAction.MoveRight), "TestFromLegacyMoveRight failed: Action mismatch");
        }

        [Test]
        public void TestFromLegacyMoveLeft()
        {
            var legacyFrame = new LegacyReplayFrame(0, new Vector2(5, 0).X, null, ReplayButtonState.None);
            var lastFrame = new CatchReplayFrame(0, 10, false);
            var catchFrame = new CatchReplayFrame();
            catchFrame.FromLegacy(legacyFrame, null, lastFrame);

            Assert.AreEqual(5, catchFrame.Position, "TestFromLegacyMoveLeft failed: Position mismatch");
            Assert.IsTrue(lastFrame.Actions.Contains(CatchAction.MoveLeft), "TestFromLegacyMoveLeft failed: Action mismatch");
        }

        [Test]
        public void TestFromLegacyNoMovement()
        {
            var legacyFrame = new LegacyReplayFrame(0, new Vector2(10, 0).X, null, ReplayButtonState.None);
            var lastFrame = new CatchReplayFrame(0, 10, false);
            var catchFrame = new CatchReplayFrame();
            catchFrame.FromLegacy(legacyFrame, null, lastFrame);

            Assert.AreEqual(10, catchFrame.Position, "TestFromLegacyNoMovement failed: Position mismatch");
            Assert.IsFalse(lastFrame.Actions.Contains(CatchAction.MoveLeft), "TestFromLegacyNoMovement failed: No unexpected MoveLeft action");
            Assert.IsFalse(lastFrame.Actions.Contains(CatchAction.MoveRight), "TestFromLegacyNoMovement failed: No unexpected MoveRight action");
        }
    }
}
