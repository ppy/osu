// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Foundation;
using osu.Framework.iOS;
using osu.Game.Tests;

namespace osu.Game.Rulesets.Mania.Tests.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        protected override Framework.Game CreateGame() => new OsuTestBrowser();
    }
}
