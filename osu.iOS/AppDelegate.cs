// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE
using Foundation;
using osu.Framework.iOS;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Framework.Allocation;

namespace osu.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.

    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        OsuGame iOSOsuGame = new OsuGame();

        protected override Framework.Game CreateGame()
        {
           return iOSOsuGame;
        }

        [Export("application:openURL:options:")]
        override public bool OpenUrl(UIKit.UIApplication app, NSUrl url, NSDictionary options)
        {
            iOSOsuGame.Import(url.Path);
            return true;
        }
    }
}