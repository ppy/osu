// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Foundation;
using osu.Framework.iOS;
using osu.Framework.Threading;
using UIKit;

namespace osu.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        private OsuGameIOS game;

        protected override Framework.Game CreateGame() => game = new OsuGameIOS();

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            game.HandleUrl(url.AbsoluteString);
            return true;
        }
    }
}
