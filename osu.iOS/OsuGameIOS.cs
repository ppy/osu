// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Foundation;
using osu.Framework.Input.Handlers;
using osu.Framework.iOS.Input;
using osu.Game;
using osu.Game.Overlays.Settings;
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

        public override SettingsSubsection CreateSettingsSubsectionFor(InputHandler handler)
        {
            switch (handler)
            {
                case IOSMouseHandler _:
                    return new IOSMouseSettings();

                default:
                    return base.CreateSettingsSubsectionFor(handler);
            }
        }

        private class IOSBatteryInfo : BatteryInfo
        {
            public override double ChargeLevel => Battery.ChargeLevel;

            public override bool IsCharging => Battery.PowerSource != BatteryPowerSource.Battery;
        }
    }
}
