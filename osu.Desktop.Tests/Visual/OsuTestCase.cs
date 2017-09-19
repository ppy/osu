// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Desktop.Platform;
using osu.Framework.Testing;
using osu.Game;

namespace osu.Desktop.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        public override void RunTest()
        {
            using (var host = new HeadlessGameHost(realtime: false))
                host.Run(new OsuTestCaseTestRunner(this));
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
