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
    public class StreamingFramedReplayInputHandlerTest
    {
        private Replay replay;
        private TestInputHandler handler;

        [SetUp]
        public void SetUp()
        {
            handler = new TestInputHandler(replay = new Replay
            {
                HasReceivedAllFrames = false,
                Frames = new List<ReplayFrame>
                {
                    new TestReplayFrame(0),
                    new TestReplayFrame(1000),
                    new TestReplayFrame(2000),
                    new TestReplayFrame(3000, true),
                    new TestReplayFrame(4000, true),
                    new TestReplayFrame(5000, true),
                    new TestReplayFrame(7000, true),
                    new TestReplayFrame(8000),
                }
            });
        }

        [Test]
        public void TestNormalPlayback()
        {
            Assert.IsNull(handler.CurrentFrame);

            confirmCurrentFrame(null);
            confirmNextFrame(0);

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
            confirmNextFrame(null);

            setTime(8200, null);
            confirmCurrentFrame(7);
            confirmNextFrame(null);

            setTime(8400, null);
            confirmCurrentFrame(7);
            confirmNextFrame(null);
        }

        [Test]
        public void TestIntroTime()
        {
            setTime(-1000, -1000);
            confirmCurrentFrame(null);
            confirmNextFrame(0);

            setTime(-500, -500);
            confirmCurrentFrame(null);
            confirmNextFrame(0);

            setTime(0, 0);
            confirmCurrentFrame(0);
            confirmNextFrame(1);
        }

        [Test]
        public void TestBasicRewind()
        {
            setTime(2800, 0);
            setTime(2800, 1000);
            setTime(2800, 2000);
            setTime(2800, 2800);
            confirmCurrentFrame(2);
            confirmNextFrame(3);

            // pivot without crossing a frame boundary
            setTime(2700, 2700);
            confirmCurrentFrame(2);
            confirmNextFrame(1);

            // cross current frame boundary; should not yet update frame
            setTime(1980, 1980);
            confirmCurrentFrame(2);
            confirmNextFrame(1);

            setTime(1200, 1200);
            confirmCurrentFrame(2);
            confirmNextFrame(1);

            // ensure each frame plays out until start
            setTime(-500, 1000);
            confirmCurrentFrame(1);
            confirmNextFrame(0);

            setTime(-500, 0);
            confirmCurrentFrame(0);
            confirmNextFrame(null);

            setTime(-500, -500);
            confirmCurrentFrame(0);
            confirmNextFrame(null);
        }

        [Test]
        public void TestRewindInsideImportantSection()
        {
            fastForwardToPoint(3000);

            setTime(4000, 4000);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(3500, null);
            confirmCurrentFrame(4);
            confirmNextFrame(3);

            setTime(3000, 3000);
            confirmCurrentFrame(3);
            confirmNextFrame(2);

            setTime(3500, null);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(4000, 4000);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(4500, null);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(4000, null);
            confirmCurrentFrame(4);
            confirmNextFrame(5);

            setTime(3500, null);
            confirmCurrentFrame(4);
            confirmNextFrame(3);

            setTime(3000, 3000);
            confirmCurrentFrame(3);
            confirmNextFrame(2);
        }

        [Test]
        public void TestRewindOutOfImportantSection()
        {
            fastForwardToPoint(3500);

            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3200, null);
            // next frame doesn't change even though direction reversed, because of important section.
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(3000, null);
            confirmCurrentFrame(3);
            confirmNextFrame(4);

            setTime(2800, 2800);
            confirmCurrentFrame(3);
            confirmNextFrame(2);
        }

        private void fastForwardToPoint(double destination)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (handler.SetFrameFromTime(destination) == null)
                    return;
            }

            throw new TimeoutException("Seek was never fulfilled");
        }

        private void setTime(double set, double? expect)
        {
            Assert.AreEqual(expect, handler.SetFrameFromTime(set));
        }

        private void confirmCurrentFrame(int? frame)
        {
            if (frame.HasValue)
            {
                Assert.IsNotNull(handler.CurrentFrame);
                Assert.AreEqual(replay.Frames[frame.Value].Time, handler.CurrentFrame.Time);
            }
            else
            {
                Assert.IsNull(handler.CurrentFrame);
            }
        }

        private void confirmNextFrame(int? frame)
        {
            if (frame.HasValue)
            {
                Assert.IsNotNull(handler.NextFrame);
                Assert.AreEqual(replay.Frames[frame.Value].Time, handler.NextFrame.Time);
            }
            else
            {
                Assert.IsNull(handler.NextFrame);
            }
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
