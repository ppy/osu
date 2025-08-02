// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Microsoft.Maui.Devices;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Screens;
using osu.Game.Updater;
using osu.Game.Utils;
using osuTK;

namespace osu.Android
{
    public partial class OsuGameAndroid : OsuGame
    {
        [Cached]
        private readonly OsuGameActivity gameActivity;

        private readonly PackageInfo packageInfo;

        public override Vector2 ScalingContainerTargetDrawSize => new Vector2(1024, 1024 * DrawHeight / DrawWidth);

        public OsuGameAndroid(OsuGameActivity activity)
            : base(null)
        {
            gameActivity = activity;
            packageInfo = Application.Context.ApplicationContext!.PackageManager!.GetPackageInfo(Application.Context.ApplicationContext.PackageName!, 0).AsNonNull();
        }

        public override string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebugBuild ? @"debug" : @"release");

                return packageInfo.VersionName.AsNonNull();
            }
        }

        public override Version AssemblyVersion => new Version(packageInfo.VersionName.AsNonNull().Split('-').First());

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UserPlayingState.BindValueChanged(_ => updateOrientation());
        }

        protected override void ScreenChanged(IOsuScreen? current, IOsuScreen? newScreen)
        {
            base.ScreenChanged(current, newScreen);

            if (newScreen != null)
                updateOrientation();
        }

        private void updateOrientation()
        {
            var orientation = MobileUtils.GetOrientation(this, (IOsuScreen)ScreenStack.CurrentScreen, gameActivity.IsTablet);

            switch (orientation)
            {
                case MobileUtils.Orientation.Locked:
                    gameActivity.RequestedOrientation = ScreenOrientation.Locked;
                    break;

                case MobileUtils.Orientation.Portrait:
                    gameActivity.RequestedOrientation = ScreenOrientation.Portrait;
                    break;

                case MobileUtils.Orientation.Default:
                    gameActivity.RequestedOrientation = gameActivity.DefaultOrientation;
                    break;
            }
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.CursorState |= CursorState.Hidden;
        }

        protected override UpdateManager CreateUpdateManager() => new MobileUpdateNotifier();

        protected override BatteryInfo CreateBatteryInfo() => new AndroidBatteryInfo();

        private class AndroidBatteryInfo : BatteryInfo
        {
            public override double? ChargeLevel => Battery.ChargeLevel;

            public override bool OnBattery => Battery.PowerSource == BatteryPowerSource.Battery;
        }
    }
}
