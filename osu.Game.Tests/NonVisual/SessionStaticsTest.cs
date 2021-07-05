// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Configuration;
using osu.Game.Input;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class SessionStaticsTest
    {
        private SessionStatics sessionStatics;
        private IdleTracker sessionIdleTracker;

        [SetUp]
        public void SetUp()
        {
            sessionStatics = new SessionStatics();
            sessionIdleTracker = new GameIdleTracker(1000);

            sessionStatics.SetValue(Static.LoginOverlayDisplayed, true);
            sessionStatics.SetValue(Static.MutedAudioNotificationShownOnce, true);
            sessionStatics.SetValue(Static.LowBatteryNotificationShownOnce, true);
            sessionStatics.SetValue(Static.LastHoverSoundPlaybackTime, (double?)1d);

            sessionIdleTracker.IsIdle.BindValueChanged(e =>
            {
                if (e.NewValue)
                    sessionStatics.ResetValues();
            });
        }

        [Test]
        [Timeout(2000)]
        public void TestSessionStaticsReset()
        {
            sessionIdleTracker.IsIdle.BindValueChanged(e =>
            {
                Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.LoginOverlayDisplayed).IsDefault);
                Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).IsDefault);
                Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce).IsDefault);
                Assert.IsTrue(sessionStatics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime).IsDefault);
            });
        }
    }
}
