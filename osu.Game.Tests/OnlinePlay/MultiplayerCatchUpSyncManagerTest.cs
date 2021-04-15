// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.OnlinePlay
{
    [HeadlessTest]
    public class MultiplayerCatchUpSyncManagerTest : OsuTestScene
    {
        private TestManualClock master;
        private SpectatorCatchUpSyncManager syncManager;

        private TestSpectatorSlaveClock slave1;
        private TestSpectatorSlaveClock slave2;

        [SetUp]
        public void Setup()
        {
            syncManager = new SpectatorCatchUpSyncManager(master = new TestManualClock());
            syncManager.AddSlave(slave1 = new TestSpectatorSlaveClock(1));
            syncManager.AddSlave(slave2 = new TestSpectatorSlaveClock(2));

            Schedule(() => Child = syncManager);
        }

        [Test]
        public void TestMasterClockStartsWhenAllSlavesHaveFrames()
        {
            setWaiting(() => slave1, false);
            assertMasterState(false);
            assertSlaveState(() => slave1, false);
            assertSlaveState(() => slave2, false);

            setWaiting(() => slave2, false);
            assertMasterState(true);
            assertSlaveState(() => slave1, true);
            assertSlaveState(() => slave2, true);
        }

        [Test]
        public void TestMasterClockDoesNotStartWhenNoneReadyForMaximumDelayTime()
        {
            AddWaitStep($"wait {SpectatorCatchUpSyncManager.MAXIMUM_START_DELAY} milliseconds", (int)Math.Ceiling(SpectatorCatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));
            assertMasterState(false);
        }

        [Test]
        public void TestMasterClockStartsWhenAnyReadyForMaximumDelayTime()
        {
            setWaiting(() => slave1, false);
            AddWaitStep($"wait {SpectatorCatchUpSyncManager.MAXIMUM_START_DELAY} milliseconds", (int)Math.Ceiling(SpectatorCatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));
            assertMasterState(true);
        }

        [Test]
        public void TestSlaveDoesNotCatchUpWhenSlightlyOutOfSync()
        {
            setAllWaiting(false);

            setMasterTime(SpectatorCatchUpSyncManager.SYNC_TARGET + 1);
            assertCatchingUp(() => slave1, false);
        }

        [Test]
        public void TestSlaveStartsCatchingUpWhenTooFarBehind()
        {
            setAllWaiting(false);

            setMasterTime(SpectatorCatchUpSyncManager.MAX_SYNC_OFFSET + 1);
            assertCatchingUp(() => slave1, true);
            assertCatchingUp(() => slave2, true);
        }

        [Test]
        public void TestSlaveKeepsCatchingUpWhenSlightlyOutOfSync()
        {
            setAllWaiting(false);

            setMasterTime(SpectatorCatchUpSyncManager.MAX_SYNC_OFFSET + 1);
            setSlaveTime(() => slave1, SpectatorCatchUpSyncManager.SYNC_TARGET + 1);
            assertCatchingUp(() => slave1, true);
        }

        [Test]
        public void TestSlaveStopsCatchingUpWhenInSync()
        {
            setAllWaiting(false);

            setMasterTime(SpectatorCatchUpSyncManager.MAX_SYNC_OFFSET + 2);
            setSlaveTime(() => slave1, SpectatorCatchUpSyncManager.SYNC_TARGET);
            assertCatchingUp(() => slave1, false);
            assertCatchingUp(() => slave2, true);
        }

        [Test]
        public void TestSlaveDoesNotStopWhenSlightlyAhead()
        {
            setAllWaiting(false);

            setSlaveTime(() => slave1, -SpectatorCatchUpSyncManager.SYNC_TARGET);
            assertCatchingUp(() => slave1, false);
            assertSlaveState(() => slave1, true);
        }

        [Test]
        public void TestSlaveStopsWhenTooFarAheadAndStartsWhenBackInSync()
        {
            setAllWaiting(false);

            setSlaveTime(() => slave1, -SpectatorCatchUpSyncManager.SYNC_TARGET - 1);

            // This is a silent catchup, where IsCatchingUp = false but IsRunning = false also.
            assertCatchingUp(() => slave1, false);
            assertSlaveState(() => slave1, false);

            setMasterTime(1);
            assertCatchingUp(() => slave1, false);
            assertSlaveState(() => slave1, true);
        }

        [Test]
        public void TestInSyncSlaveDoesNotStartIfWaitingOnFrames()
        {
            setAllWaiting(false);

            assertSlaveState(() => slave1, true);
            setWaiting(() => slave1, true);
            assertSlaveState(() => slave1, false);
        }

        private void setWaiting(Func<TestSpectatorSlaveClock> slave, bool waiting)
            => AddStep($"set slave {slave().Id} waiting = {waiting}", () => slave().WaitingOnFrames.Value = waiting);

        private void setAllWaiting(bool waiting) => AddStep($"set all slaves waiting = {waiting}", () =>
        {
            slave1.WaitingOnFrames.Value = waiting;
            slave2.WaitingOnFrames.Value = waiting;
        });

        private void setMasterTime(double time)
            => AddStep($"set master = {time}", () => master.Seek(time));

        /// <summary>
        /// slave.Time = master.Time - offsetFromMaster
        /// </summary>
        private void setSlaveTime(Func<TestSpectatorSlaveClock> slave, double offsetFromMaster)
            => AddStep($"set slave {slave().Id} = master - {offsetFromMaster}", () => slave().Seek(master.CurrentTime - offsetFromMaster));

        private void assertMasterState(bool running)
            => AddAssert($"master clock {(running ? "is" : "is not")} running", () => master.IsRunning == running);

        private void assertCatchingUp(Func<TestSpectatorSlaveClock> slave, bool catchingUp) =>
            AddAssert($"slave {slave().Id} {(catchingUp ? "is" : "is not")} catching up", () => slave().IsCatchingUp == catchingUp);

        private void assertSlaveState(Func<TestSpectatorSlaveClock> slave, bool running)
            => AddAssert($"slave {slave().Id} {(running ? "is" : "is not")} running", () => slave().IsRunning == running);

        private class TestSpectatorSlaveClock : TestManualClock, ISpectatorSlaveClock
        {
            public readonly Bindable<bool> WaitingOnFrames = new Bindable<bool>(true);
            IBindable<bool> ISpectatorSlaveClock.WaitingOnFrames => WaitingOnFrames;

            public bool IsCatchingUp { get; set; }

            public readonly int Id;

            public TestSpectatorSlaveClock(int id)
            {
                Id = id;

                WaitingOnFrames.BindValueChanged(waiting =>
                {
                    if (waiting.NewValue)
                        Stop();
                    else
                        Start();
                });
            }

            public void ProcessFrame()
            {
            }

            public double ElapsedFrameTime => 0;

            public double FramesPerSecond => 0;

            public FrameTimeInfo TimeInfo => default;
        }

        private class TestManualClock : ManualClock, IAdjustableClock
        {
            public void Start() => IsRunning = true;

            public void Stop() => IsRunning = false;

            public bool Seek(double position)
            {
                CurrentTime = position;
                return true;
            }

            public void Reset()
            {
            }

            public void ResetSpeedAdjustments()
            {
            }
        }
    }
}
