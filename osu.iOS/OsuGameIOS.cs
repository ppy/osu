// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Foundation;
using osu.Game;
using osu.Game.Updater;
using osu.Game.Utils;
using Xamarin.Essentials;

namespace osu.iOS
{
    public class OsuGameIOS : OsuGame
    {
        public override Version AssemblyVersion => new Version(NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString());

        protected override UpdateManager CreateUpdateManager() => new SimpleUpdateManager();

        protected override BatteryInfo CreateBatteryInfo() => new IOSBatteryInfo();

        private class IOSBatteryInfo : BatteryInfo
        {
            public override double ChargeLevel => Battery.ChargeLevel;

            public override bool IsCharging => Battery.PowerSource != BatteryPowerSource.Battery;
        }
    }
}
