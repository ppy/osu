// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests.Responses;

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
            sessionStatics.SetValue(Static.SeasonalBackgrounds, new APISeasonalBackgrounds { EndDate = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero) });

            Assert.IsFalse(sessionStatics.GetBindable<bool>(Static.LoginOverlayDisplayed).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<APISeasonalBackgrounds>(Static.SeasonalBackgrounds).IsDefault);

            sessionStatics.ResetAfterInactivity();

            Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.LoginOverlayDisplayed).IsDefault);
            Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).IsDefault);
            Assert.IsTrue(sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce).IsDefault);
            // some statics should not reset despite inactivity.
            Assert.IsFalse(sessionStatics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime).IsDefault);
            Assert.IsFalse(sessionStatics.GetBindable<APISeasonalBackgrounds>(Static.SeasonalBackgrounds).IsDefault);
        }
    }
}
