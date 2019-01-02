// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE
using System.Threading.Tasks;
using Foundation;
using osu.Framework.iOS;
using osu.Game;
using osuTK.Input;

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

        override public void fileDrop(object sender, FileDropEventArgs e)
        {
            var filePaths = new[] { e.FileName };

            Task.Factory.StartNew(() => iOSOsuGame.Import(filePaths), TaskCreationOptions.LongRunning);
        }


    }
}