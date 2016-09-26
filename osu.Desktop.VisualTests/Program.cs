﻿// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Desktop;
using osu.Desktop.KeyCounterTutorial;
using osu.Framework.Desktop;
using osu.Framework.OS;

namespace osu.Framework.VisualTests
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BasicGameHost host = Host.GetSuitableHost();
            host.Load(new TestCaseKeyCounter());
            host.Run();
        }
    }
}
