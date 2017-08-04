using NUnit.Framework;
using osu.Framework.Desktop.Platform;
using osu.Framework.Testing;
using osu.Game;

namespace osu.Desktop.Tests.Visual
{
    [TestFixture]
    public abstract class OsuTestCase : TestCase
    {
        [Test]
        public override void RunTest()
        {
            using (var host = new HeadlessGameHost())
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
