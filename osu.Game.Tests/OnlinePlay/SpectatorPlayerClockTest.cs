// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.OnlinePlay
{
    [HeadlessTest]
    public partial class SpectatorPlayerClockTest : OsuTestScene
    {
        private ManualClock masterSource = null!;
        private FramedClock master = null!;
        private SpectatorPlayerClock clock = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            master = new FramedClock(masterSource = new ManualClock { IsRunning = true });
            clock = new SpectatorPlayerClock(master) { IsRunning = true };
        });

        [Test]
        public void TestRateIncludesMasterRate()
        {
            setMasterRate(1.5);
            assertRate(1.5);

            setCatchingUp(true);
            assertRate(1.5 * SpectatorPlayerClock.CATCHUP_RATE);
        }

        [Test]
        public void TestProcessFrameDoesNotReapplyMasterRate()
        {
            // master clock elapsed time already accounts for rate-adjust, so ProcessFrame must not scale elapsed time
            // by the master rate a second time
            setMasterRate(1.5);
            setMasterElapsed(20);

            processFrame();

            assertCurrentTime(20);
        }

        [Test]
        public void TestProcessFrameAppliesCatchUpButNotMasterRate()
        {
            setMasterRate(1.5);
            setMasterElapsed(20);
            setCatchingUp(true);

            processFrame();

            assertCurrentTime(20 * SpectatorPlayerClock.CATCHUP_RATE);
        }

        private void setMasterRate(double rate)
            => AddStep($"set master rate = {rate}", () => masterSource.Rate = rate);

        private void setMasterElapsed(double elapsed)
            => AddStep($"set master elapsed = {elapsed}", () =>
            {
                masterSource.CurrentTime += elapsed;
                master.ProcessFrame();
            });

        private void setCatchingUp(bool catchingUp)
            => AddStep($"set catching up = {catchingUp}", () => clock.IsCatchingUp = catchingUp);

        private void processFrame()
            => AddStep("ProcessFrame", () => clock.ProcessFrame());

        private void assertRate(double expected)
            => AddAssert($"rate is {expected}", () => clock.Rate, () => Is.EqualTo(expected));

        private void assertCurrentTime(double expected)
            => AddAssert($"current time is {expected}", () => clock.CurrentTime, () => Is.EqualTo(expected));
    }
}
