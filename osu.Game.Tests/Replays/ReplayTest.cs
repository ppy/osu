// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Tests.Replays
{
    [TestFixture]
    public class ReplayTest
    {
        [Test]
        public void TestReplay()
        {
            var replay = new Replay(new[]
            {
                new ReplayFrame(2000),
                new ReplayFrame(1000),
                new ReplayFrame(1000),
                new ReplayFrame(3000),
            });
            Assert.That(replay.Frames, Is.Ordered.Ascending.By(nameof(ReplayFrame.Time)));
            Assert.IsTrue(replay.IsComplete);

            Assert.IsEmpty(new Replay().Frames);
            Assert.IsTrue(new Replay().IsComplete);
        }

        [Test]
        public void TestStreamingReplay()
        {
            var replay = new StreamingReplay();
            replay.Add(new ReplayFrame(-1000));
            replay.Add(new ReplayFrame(0));
            replay.Add(new ReplayFrame(0));
            Assert.AreEqual(replay.Frames.Count, 3);
            Assert.IsFalse(replay.IsComplete);

            Assert.Throws<ArgumentException>(() => replay.Add(new ReplayFrame(-1)));
            Assert.Throws<ArgumentException>(() => replay.Add(new ReplayFrame(double.NaN)));

            replay.MarkCompleted();
            Assert.IsTrue(replay.IsComplete);

            Assert.Throws<InvalidOperationException>(() => replay.Add(new ReplayFrame(1000)));
            Assert.AreEqual(replay.Frames.Count, 3);
        }

        [Test]
        public void TestReplayFramesSortStability()
        {
            const double repeating_time = 5000;

            // add a collection of frames in shuffled order time-wise; each frame also stores its original index to check stability later.
            // data is hand-picked and breaks if the unstable List<T>.Sort() is used.
            // in theory this can still return a false-positive with another unstable algorithm if extremely unlucky,
            // but there is no conceivable fool-proof way to prevent that anyways.
            var replay = new Replay(new[]
            {
                repeating_time,
                0,
                3000,
                repeating_time,
                repeating_time,
                6000,
                9000,
                repeating_time,
                repeating_time,
                1000,
                11000,
                21000,
                4000,
                repeating_time,
                repeating_time,
                8000,
                2000,
                7000,
                repeating_time,
                repeating_time,
                10000
            }.Select((time, index) => new TestReplayFrame(time, index)));

            // ensure sort stability by checking that the frames with time == repeating_time are sorted in ascending frame index order themselves.
            var repeatingTimeFramesData = replay.Frames
                                                .Cast<TestReplayFrame>()
                                                .Where(f => f.Time == repeating_time)
                                                .Select(f => f.FrameIndex);

            Assert.That(repeatingTimeFramesData, Is.Ordered.Ascending);
        }

        private class TestReplayFrame : ReplayFrame
        {
            public readonly int FrameIndex;

            public TestReplayFrame(double time, int frameIndex)
                : base(time)
            {
                FrameIndex = frameIndex;
            }
        }
    }
}
