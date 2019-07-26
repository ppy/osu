// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using Foundation;
using osu.Framework.iOS;
using osu.Game;
using UIKit;

namespace osu.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        private OsuGameIOS game;

        protected override Framework.Game CreateGame() => game = new OsuGameIOS();

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            Task.Run(() => game.Import(url.Path));
            return true;
        }
    }
}
