﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game;

namespace osu.Desktop.VisualTests
{
    public class Benchmark : OsuGameBase
    {
        private const double time_per_test = 200;

        [BackgroundDependencyLoader]
        private void load()
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

            Console.WriteLine($@"{Time}: Running {f.TestCount} tests for {time_per_test}ms each...");

            for (int i = 1; i < f.TestCount; i++)
            {
                int loadableCase = i;
                Scheduler.AddDelayed(delegate
                {
                    f.LoadTest(loadableCase);
                    Console.WriteLine($@"{Time}: Switching to test #{loadableCase}");
                }, loadableCase * time_per_test);
            }

            Scheduler.AddDelayed(Host.Exit, f.TestCount * time_per_test);
        }
    }
}
