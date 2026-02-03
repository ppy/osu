// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui.Devices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Configuration;
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

        private Native.VulkanRenderer? vulkanRenderer;
        private Native.OboeAudio? oboeAudio;

        private readonly global::Android.Content.PM.PackageInfo packageInfo;

        public override Vector2 ScalingContainerTargetDrawSize => new Vector2(1024, 1024 * DrawHeight / DrawWidth);

        public OsuGameAndroid(OsuGameActivity activity)
            : base(null)
        {
            gameActivity = activity;
            packageInfo = global::Android.App.Application.Context.ApplicationContext!.PackageManager!.GetPackageInfo(global::Android.App.Application.Context.ApplicationContext.PackageName!, 0).AsNonNull();
        }

        public void HandleStylusInput(float x, float y, long timestampNano)
        {
            // Late-input sampling and reprojection into audio timeline would happen here.
            _ = x;
            _ = y;
            _ = timestampNano;
            Debug.WriteLine($"Stylus: {x}, {y}, {timestampNano}");
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

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.PerformanceMode, PerformanceMode);
            config.BindWith(OsuSetting.UseAngle, UseAngle);
        }

        public readonly Bindable<bool> PerformanceMode = new Bindable<bool>();

        public readonly Bindable<bool> UseAngle = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UserPlayingState.BindValueChanged(_ => updateOrientation());
            PerformanceMode.BindValueChanged(enabled =>
            {
                gameActivity.ApplyPerformanceOptimizations(enabled.NewValue);

                if (enabled.NewValue)
                {
                    try
                    {
                        vulkanRenderer ??= new Native.VulkanRenderer();
                        oboeAudio ??= new Native.OboeAudio();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to initialize native components: {ex}");
                    }
                }
            }, true);
            UseAngle.BindValueChanged(enabled => gameActivity.ApplyAngleOptimizations(enabled.NewValue), true);
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
                    gameActivity.RequestedOrientation = global::Android.Content.PM.ScreenOrientation.Locked;
                    break;

                case MobileUtils.Orientation.Portrait:
                    gameActivity.RequestedOrientation = global::Android.Content.PM.ScreenOrientation.Portrait;
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            vulkanRenderer?.Dispose();
            oboeAudio?.Dispose();
        }

        private class AndroidBatteryInfo : BatteryInfo
        {
            public override double? ChargeLevel => Battery.ChargeLevel;

            public override bool OnBattery => Battery.PowerSource == BatteryPowerSource.Battery;
        }
    }
}
