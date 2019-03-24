// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
                if (args.FirstOrDefault() == "--benchmark")
                    host.Run(new Benchmark());
                else
                    host.Run(new OsuTestBrowser());
                return 0;
            }
        }
    }
}
