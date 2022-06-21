// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.OnlinePlay
{
    [HeadlessTest]
    public class TestSceneCatchUpSyncManager : OsuTestScene
    {
        private TestManualClock master;
        private CatchUpSyncManager syncManager;

        private TestSpectatorPlayerClock player1;
        private TestSpectatorPlayerClock player2;

        [SetUp]
        public void Setup()
        {
            syncManager = new CatchUpSyncManager(master = new TestManualClock());
            syncManager.AddPlayerClock(player1 = new TestSpectatorPlayerClock(1));
            syncManager.AddPlayerClock(player2 = new TestSpectatorPlayerClock(2));

            Schedule(() => Child = syncManager);
        }

        [Test]
        public void TestPlayerClocksStartWhenAllHaveFrames()
        {
            setWaiting(() => player1, false);
            assertPlayerClockState(() => player1, false);
            assertPlayerClockState(() => player2, false);

            setWaiting(() => player2, false);
            assertPlayerClockState(() => player1, true);
            assertPlayerClockState(() => player2, true);
        }

        [Test]
        public void TestReadyPlayersStartWhenReadyForMaximumDelayTime()
        {
            setWaiting(() => player1, false);
            AddWaitStep($"wait {CatchUpSyncManager.MAXIMUM_START_DELAY} milliseconds", (int)Math.Ceiling(CatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));
            assertPlayerClockState(() => player1, true);
            assertPlayerClockState(() => player2, false);
        }

        [Test]
        public void TestPlayerClockDoesNotCatchUpWhenSlightlyOutOfSync()
        {
            setAllWaiting(false);

            setMasterTime(CatchUpSyncManager.SYNC_TARGET + 1);
            assertCatchingUp(() => player1, false);
        }

        [Test]
        public void TestPlayerClockStartsCatchingUpWhenTooFarBehind()
        {
            setAllWaiting(false);

            setMasterTime(CatchUpSyncManager.MAX_SYNC_OFFSET + 1);
            assertCatchingUp(() => player1, true);
            assertCatchingUp(() => player2, true);
        }

        [Test]
        public void TestPlayerClockKeepsCatchingUpWhenSlightlyOutOfSync()
        {
            setAllWaiting(false);

            setMasterTime(CatchUpSyncManager.MAX_SYNC_OFFSET + 1);
            setPlayerClockTime(() => player1, CatchUpSyncManager.SYNC_TARGET + 1);
            assertCatchingUp(() => player1, true);
        }

        [Test]
        public void TestPlayerClockStopsCatchingUpWhenInSync()
        {
            setAllWaiting(false);

            setMasterTime(CatchUpSyncManager.MAX_SYNC_OFFSET + 2);
            setPlayerClockTime(() => player1, CatchUpSyncManager.SYNC_TARGET);
            assertCatchingUp(() => player1, false);
            assertCatchingUp(() => player2, true);
        }

        [Test]
        public void TestPlayerClockDoesNotStopWhenSlightlyAhead()
        {
            setAllWaiting(false);

            setPlayerClockTime(() => player1, -CatchUpSyncManager.SYNC_TARGET);
            assertCatchingUp(() => player1, false);
            assertPlayerClockState(() => player1, true);
        }

        [Test]
        public void TestPlayerClockStopsWhenTooFarAheadAndStartsWhenBackInSync()
        {
            setAllWaiting(false);

            setPlayerClockTime(() => player1, -CatchUpSyncManager.SYNC_TARGET - 1);

            // This is a silent catchup, where IsCatchingUp = false but IsRunning = false also.
            assertCatchingUp(() => player1, false);
            assertPlayerClockState(() => player1, false);

            setMasterTime(1);
            assertCatchingUp(() => player1, false);
            assertPlayerClockState(() => player1, true);
        }

        [Test]
        public void TestInSyncPlayerClockDoesNotStartIfWaitingOnFrames()
        {
            setAllWaiting(false);

            assertPlayerClockState(() => player1, true);
            setWaiting(() => player1, true);
            assertPlayerClockState(() => player1, false);
        }

        private void setWaiting(Func<TestSpectatorPlayerClock> playerClock, bool waiting)
            => AddStep($"set player clock {playerClock().Id} waiting = {waiting}", () => playerClock().WaitingOnFrames.Value = waiting);

        private void setAllWaiting(bool waiting) => AddStep($"set all player clocks waiting = {waiting}", () =>
        {
            player1.WaitingOnFrames.Value = waiting;
            player2.WaitingOnFrames.Value = waiting;
        });

        private void setMasterTime(double time)
            => AddStep($"set master = {time}", () => master.Seek(time));

        /// <summary>
        /// clock.Time = master.Time - offsetFromMaster
        /// </summary>
        private void setPlayerClockTime(Func<TestSpectatorPlayerClock> playerClock, double offsetFromMaster)
            => AddStep($"set player clock {playerClock().Id} = master - {offsetFromMaster}", () => playerClock().Seek(master.CurrentTime - offsetFromMaster));

        private void assertCatchingUp(Func<TestSpectatorPlayerClock> playerClock, bool catchingUp) =>
            AddAssert($"player clock {playerClock().Id} {(catchingUp ? "is" : "is not")} catching up", () => playerClock().IsCatchingUp == catchingUp);

        private void assertPlayerClockState(Func<TestSpectatorPlayerClock> playerClock, bool running)
            => AddAssert($"player clock {playerClock().Id} {(running ? "is" : "is not")} running", () => playerClock().IsRunning == running);

        private class TestSpectatorPlayerClock : TestManualClock, ISpectatorPlayerClock
        {
            public Bindable<bool> WaitingOnFrames { get; } = new Bindable<bool>(true);

            public bool IsCatchingUp { get; set; }

            public IFrameBasedClock Source
            {
                set => throw new NotImplementedException();
            }

            public readonly int Id;

            public TestSpectatorPlayerClock(int id)
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
            public TestManualClock()
            {
                IsRunning = true;
            }

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
