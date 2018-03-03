// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Reflection;
using osu.Framework.Testing;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        public override void RunTest()
        {
            using (var host = new CleanRunHeadlessGameHost($"test-{Guid.NewGuid()}", realtime: false))
                host.Run(new OsuTestCaseTestRunner(this));
        }

        public class OsuTestCaseTestRunner : OsuGameBase
        {
            private readonly OsuTestCase testCase;

            protected override string MainResourceFile => File.Exists(base.MainResourceFile) ? base.MainResourceFile : Assembly.GetExecutingAssembly().Location;

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
