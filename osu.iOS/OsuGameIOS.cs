// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Foundation;
using Microsoft.Maui.Devices;
using osu.Framework.Graphics;
using osu.Framework.iOS;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Screens;
using osu.Game.Updater;
using osu.Game.Utils;
using osuTK;
using UIKit;

namespace osu.iOS
{
    public partial class OsuGameIOS : OsuGame
    {
        private readonly AppDelegate appDelegate;
        public override Version AssemblyVersion => new Version(NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString());

        public override bool HideUnlicensedContent => true;

        public override Vector2 ScalingContainerTargetDrawSize => new Vector2(1024, 1024 * DrawHeight / DrawWidth);

        public OsuGameIOS(AppDelegate appDelegate)
        {
            this.appDelegate = appDelegate;
        }

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

        private void updateOrientation() => UIApplication.SharedApplication.InvokeOnMainThread(() =>
        {
            bool iPad = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
            var orientation = MobileUtils.GetOrientation(this, (IOsuScreen)ScreenStack.CurrentScreen, iPad);

            switch (orientation)
            {
                case MobileUtils.Orientation.Locked:
                    appDelegate.Orientations = (UIInterfaceOrientationMask)(1 << (int)appDelegate.CurrentOrientation);
                    break;

                case MobileUtils.Orientation.Portrait:
                    appDelegate.Orientations = UIInterfaceOrientationMask.Portrait;
                    break;

                case MobileUtils.Orientation.Default:
                    appDelegate.Orientations = null;
                    break;
            }
        });

        protected override UpdateManager CreateUpdateManager() => new MobileUpdateNotifier();

        protected override BatteryInfo CreateBatteryInfo() => new IOSBatteryInfo();

        protected override Storage CreateStorage(GameHost host, Storage defaultStorage) => new OsuStorageIOS((IOSGameHost)host, defaultStorage);

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
