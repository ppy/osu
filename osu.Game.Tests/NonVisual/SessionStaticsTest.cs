// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Configuration;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class SessionStaticsTest
    {
        private SessionStatics sessionStatics;

        [Test]
        public void TestSessionStaticsReset()
        {
            sessionStatics = new SessionStatics();

            sessionStatics.SetValue(Static.LoginOverlayDisplayed, true);
            sessionStatics.SetValue(Static.MutedAudioNotificationShownOnce, true);
            sessionStatics.SetValue(Static.LowBatteryNotificationShownOnce, true);
            sessionStatics.SetValue(Static.LastHoverSoundPlaybackTime, (double?)1d);

            Assert.IsFalse(sessionStatics.GetBindable<bool>(Static.LoginOverlayDisplayed).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime).IsDefault);

            sessionStatics.ResetAfterInactivity();

            Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.LoginOverlayDisplayed).IsDefault);
            Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).IsDefault);
            Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce).IsDefault);
            Assert.IsTrue(sessionStatics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime).IsDefault);
        }
    }
}
