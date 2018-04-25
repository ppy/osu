// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Platform;

namespace osu.Game.Tests
{
    public static class VisualTestRunner
    {
        [STAThread]
        public static int Main(string[] args)
        {
            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                host.Run(new OsuTestBrowser());
                return 0;
            }
        }
    }
}
