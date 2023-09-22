// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Foundation;
using Microsoft.Maui.Devices;
using osu.Framework.Graphics;
using osu.Game;
using osu.Game.Updater;
using osu.Game.Utils;

namespace osu.iOS
{
    public partial class OsuGameIOS : OsuGame
    {
        public override Version AssemblyVersion => new Version(NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString());

        protected override UpdateManager CreateUpdateManager() => new SimpleUpdateManager();

        protected override BatteryInfo CreateBatteryInfo() => new IOSBatteryInfo();

        protected override Edges SafeAreaOverrideEdges =>
            // iOS shows a home indicator at the bottom, and adds a safe area to account for this.
            // Because we have the home indicator (mostly) hidden we don't really care about drawing in this region.
            Edges.Bottom;

        private class IOSBatteryInfo : BatteryInfo
        {
            public override double? ChargeLevel => Battery.ChargeLevel;

            public override bool OnBattery => Battery.PowerSource == BatteryPowerSource.Battery;
        }
    }
}
