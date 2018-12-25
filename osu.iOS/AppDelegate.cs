using System;
using System.IO;
using Foundation;
using osu.Framework;
using osu.Framework.iOS;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;

namespace osu.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.

    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        protected override Framework.Game CreateGame() => new OsuGame();
        private BeatmapManager beatmap = null;

        [Export("application:openURL:options:")]

        override public bool OpenUrl(UIKit.UIApplication app, NSUrl url, NSDictionary options)
        {

            Console.WriteLine(url.Path);
            string path = url.Path;
            //Stream beatmapToLoad = new FileStream(path, FileMode.Open, FileAccess.Read);
            beatmap.Import(path);
            return true;
        }


    }


}

