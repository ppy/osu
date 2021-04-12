// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class FramedReplayInputHandlerTest
    {
        private Replay replay;
        private TestInputHandler handler;

        [SetUp]
        public void SetUp()
        {
            handler = new TestInputHandler(replay = new Replay
            {
                HasReceivedAllFrames = false
            });
        }

        [Test]
        public void TestNormalPlayback()
        {
            setReplayFrames();

            setTime(0, 0);
            confirmCurrentFrame(0);
            confirmNextFrame(1);

            // if we hit the first frame perfectly, time should progress to it.
            setTime(1000, 1000);
            confirmCurrentFrame(1);
            confirmNextFrame(2);

            // in between non-important frames should progress based on input.
            setTime(1200, 1200);
            confirmCurrentFrame(1);

            setTime(1400, 1400);
            confirmCurrentFrame(1);

            // progressing beyond the next frame should force time to that frame once.
            setTime(2200, 2000);
            confirmCurrentFrame(2);

            // second attempt should progress to input time
            setTime(2200, 2200);
            confirmCurrentFrame(2);

            // entering important section
            setTime(3000, 3000);
            confirmCurrentFrame(3);

            // cannot progress within
            setTime(3500, null);
            confirmCurrentFrame(3);

            setTime(4000, 4000);
            confirmCurrentFrame(4);

            // still cannot progress
            setTime(4500, null);
            confirmCurrentFrame(4);

            setTime(5200, 5000);
            confirmCurrentFrame(5);

            // important section AllowedImportantTimeSpan allowance
            setTime(5200, 5200);
            confirmCurrentFrame(5);

            setTime(7200, 7000);
            confirmCurrentFrame(6);

            setTime(7200, null);
            confirmCurrentFrame(6);

            // exited important section
            setTime(8200, 8000);
            confirmCurrentFrame(7);
            confirmNextFrame(7);

            setTime(8200, 8200);
            confirmCurrentFrame(7);
            confirmNextFrame(7);
        }

        [Test]
        public void TestIntroTime()
        {
            setReplayFrames();

            setTime(-1000, -1000);
            confirmCurrentFrame(0);
            confirmNextFrame(0);

            setTime(-500, -500);
            confirmCurrentFrame(0);
            confirmNextFrame(0);

            setTime(0, 0);
            confirmCurrentFrame(0);
            confirmNextFrame(1);
        }

        [Test]
        public void TestBasicRewind()
        {
            setReplayFrames();

            setTime(2800, 0);
            setTime(2800, 1000);
            setTime(2800, 2000);
            setTime(2800, 2800);
            confirmCurrentFrame(2);
            confirmNextFrame(3);

            // pivot without crossing a frame boundary
            setTime(2700, 2700);
            confirmCurrentFrame(2);
            confirmNextFrame(3);

            // cross current frame boundary
            setTime(1980, 2000);
            confirmCurrentFrame(2);
            confirmNextFrame(3);

            setTime(1200, 1200);
            confirmCurrentFrame(1);
            confirmNextFrame(2);

            // ensure each frame plays out until start
            setTime(-500, 1000);
            confirmCurrentFrame(1);
            confirmNextFrame(2);

            setTime(-500, 0);
            confirmCurrentFrame(0);
            confirmNextFrame(1);

            setTime(-500, -500);
            confirmCurrentFrame(0);
            confirmNextFrame(0);
        }

        [Test]
        public void TestRewindInsideImportantSection()
        {
            setReplayFrames();
            fastForwardToPoint(3000);

            setTime(4000, 4000);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(3500, null);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3000, 3000);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3500, null);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(4000, 4000);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(4500, null);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(4000, 4000);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(3500, null);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3000, 3000);
            confirmCurrentFrame(3);
            confirmNextFrame(4);
        }

        [Test]
        public void TestRewindOutOfImportantSection()
        {
            setReplayFrames();
            fastForwardToPoint(3500);

            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3200, null);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3000, 3000);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(2800, 2800);
            confirmCurrentFrame(2);
            confirmNextFrame(3);
        }

        [Test]
        public void TestReplayStreaming()
        {
            // no frames are arrived yet
            setTime(0, null);
            setTime(1000, null);
            Assert.IsTrue(handler.WaitingNextFrame, "Should be waiting for the first frame");

            replay.Frames.Add(new TestReplayFrame(0));
            replay.Frames.Add(new TestReplayFrame(1000));

            // should always play from beginning
            setTime(1000, 0);
            confirmCurrentFrame(0);
            Assert.IsFalse(handler.WaitingNextFrame, "Should not be waiting yet");
            setTime(1000, 1000);
            confirmCurrentFrame(1);
            confirmNextFrame(1);
            Assert.IsTrue(handler.WaitingNextFrame, "Should be waiting");

            // cannot seek beyond the last frame
            setTime(1500, null);
            confirmCurrentFrame(1);

            setTime(-100, 0);
            confirmCurrentFrame(0);

            // can seek to the point before the first frame, however
            setTime(-100, -100);
            confirmCurrentFrame(0);
            confirmNextFrame(0);

            fastForwardToPoint(1000);
            setTime(3000, null);
            replay.Frames.Add(new TestReplayFrame(2000));
            confirmCurrentFrame(1);
            setTime(1000, 1000);
            setTime(3000, 2000);
        }

        [Test]
        public void TestMultipleFramesSameTime()
        {
            replay.Frames.Add(new TestReplayFrame(0));
            replay.Frames.Add(new TestReplayFrame(0));
            replay.Frames.Add(new TestReplayFrame(1000));
            replay.Frames.Add(new TestReplayFrame(1000));
            replay.Frames.Add(new TestReplayFrame(2000));

            // forward direction is prioritized when multiple frames have the same time.
            setTime(0, 0);
            setTime(0, 0);

            setTime(2000, 1000);
            setTime(2000, 1000);

            setTime(1000, 1000);
            setTime(1000, 1000);
            setTime(-100, 1000);
            setTime(-100, 0);
            setTime(-100, 0);
            setTime(-100, -100);
        }

        private void setReplayFrames()
        {
            replay.Frames = new List<ReplayFrame>
            {
                new TestReplayFrame(0),
                new TestReplayFrame(1000),
                new TestReplayFrame(2000),
                new TestReplayFrame(3000, true),
                new TestReplayFrame(4000, true),
                new TestReplayFrame(5000, true),
                new TestReplayFrame(7000, true),
                new TestReplayFrame(8000),
            };
            replay.HasReceivedAllFrames = true;
        }

        private void fastForwardToPoint(double destination)
        {
            for (int i = 0; i < 1000; i++)
            {
                var time = handler.SetFrameFromTime(destination);
                if (time == null || time == destination)
                    return;
            }

            throw new TimeoutException("Seek was never fulfilled");
        }

        private void setTime(double set, double? expect)
        {
            Assert.AreEqual(expect, handler.SetFrameFromTime(set), "Unexpected return value");
        }

        private void confirmCurrentFrame(int frame)
        {
            Assert.AreEqual(replay.Frames[frame].Time, handler.CurrentFrame.Time, "Unexpected current frame");
        }

        private void confirmNextFrame(int frame)
        {
            Assert.AreEqual(replay.Frames[frame].Time, handler.NextFrame.Time, "Unexpected next frame");
        }

        private class TestReplayFrame : ReplayFrame
        {
            public readonly bool IsImportant;

            public TestReplayFrame(double time, bool isImportant = false)
                : base(time)
            {
                IsImportant = isImportant;
            }
        }

        private class TestInputHandler : FramedReplayInputHandler<TestReplayFrame>
        {
            public TestInputHandler(Replay replay)
                : base(replay)
            {
                FrameAccuratePlayback = true;
            }

            protected override double AllowedImportantTimeSpan => 1000;

            protected override bool IsImportant(TestReplayFrame frame) => frame.IsImportant;
        }
    }
}
