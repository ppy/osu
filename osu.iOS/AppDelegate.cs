using System;
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

        private BeatmapManager beatmap;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            beatmap = beatmapManager;
        }



        [Export("application:openURL:options:")]
        override public bool OpenUrl(UIKit.UIApplication app, NSUrl url, NSDictionary options)
        {
            Console.WriteLine(url.Path);
            string path = url.Path;
            //Stream beatmapToLoad = new FileStream(path, FileMode.Open, FileAccess.Read);
            //workingGame.Import(path);
            iOSOsuGame.Import(path);
            return true;
        }


    }


}

