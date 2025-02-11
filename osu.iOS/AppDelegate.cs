// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Foundation;
using osu.Framework.iOS;
using UIKit;

namespace osu.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : GameApplicationDelegate
    {
        private UIInterfaceOrientationMask? defaultOrientationsMask;
        private UIInterfaceOrientationMask? orientations;

        /// <summary>
        /// The current orientation the game is displayed in.
        /// </summary>
        public UIInterfaceOrientation CurrentOrientation => Host.Window.UIWindow.WindowScene!.InterfaceOrientation;

        /// <summary>
        /// Controls the orientations allowed for the device to rotate to, overriding the default allowed orientations.
        /// </summary>
        public UIInterfaceOrientationMask? Orientations
        {
            get => orientations;
            set
            {
                if (orientations == value)
                    return;

                orientations = value;

                if (OperatingSystem.IsIOSVersionAtLeast(16))
                    Host.Window.ViewController.SetNeedsUpdateOfSupportedInterfaceOrientations();
                else
                    UIViewController.AttemptRotationToDeviceOrientation();
            }
        }

        protected override Framework.Game CreateGame() => new OsuGameIOS(this);

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
        {
            if (orientations != null)
                return orientations.Value;

            if (defaultOrientationsMask == null)
            {
                defaultOrientationsMask = 0;
                var defaultOrientations = (NSArray)NSBundle.MainBundle.ObjectForInfoDictionary("UISupportedInterfaceOrientations");

                foreach (var value in defaultOrientations.ToArray<NSString>())
                    defaultOrientationsMask |= Enum.Parse<UIInterfaceOrientationMask>(value.ToString().Replace("UIInterfaceOrientation", string.Empty));
            }

            return defaultOrientationsMask.Value;
        }
    }
}
