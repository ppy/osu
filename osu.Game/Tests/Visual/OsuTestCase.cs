// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Reflection;
using osu.Framework.Testing;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        protected override ITestCaseTestRunner CreateRunner() => new OsuTestCaseTestRunner();

        public class OsuTestCaseTestRunner : OsuGameBase, ITestCaseTestRunner
        {
            protected override string MainResourceFile => File.Exists(base.MainResourceFile) ? base.MainResourceFile : Assembly.GetExecutingAssembly().Location;

            private readonly TestCaseTestRunner.TestRunner runner;

            public OsuTestCaseTestRunner()
            {
                Add(runner = new TestCaseTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestCase test) => runner.RunTestBlocking(test);
        }
    }
}
