﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        public override void RunTest()
        {
            using (var host = new HeadlessGameHost($"test-{Guid.NewGuid()}", realtime: false))
            {
                host.Storage.DeleteDirectory(string.Empty);
                host.Run(new OsuTestCaseTestRunner(this));
            }
        }

        public class OsuTestCaseTestRunner : OsuGameBase
        {
            private readonly OsuTestCase testCase;

            public OsuTestCaseTestRunner(OsuTestCase testCase)
            {
                this.testCase = testCase;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Add(new TestCaseTestRunner.TestRunner(testCase));
            }
        }
    }
}
