// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Desktop.Platform;
using osu.Framework.GameModes.Testing;
using osu.Game;
using osu.Game.Modes;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;

namespace osu.Desktop.VisualTests
{
    public class Benchmark : OsuGameBase
    {
        private double timePerTest = 200;

        [BackgroundDependencyLoader]
        private void load(BaseGame game)
        {
            Host.MaximumDrawHz = int.MaxValue;
            Host.MaximumUpdateHz = int.MaxValue;
            Host.MaximumInactiveHz = int.MaxValue;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            TestBrowser f = new TestBrowser();
            Add(f);

            Console.WriteLine($@"{Time}: Running {f.TestCount} tests for {timePerTest}ms each...");

            for (int i = 1; i < f.TestCount; i++)
            {
                int loadableCase = i;
                Scheduler.AddDelayed(delegate
                {
                    f.LoadTest(loadableCase);
                    Console.WriteLine($@"{Time}: Switching to test #{loadableCase}");
                }, loadableCase * timePerTest);
            }

            Scheduler.AddDelayed(Host.Exit, f.TestCount * timePerTest);
        }
    }
}
