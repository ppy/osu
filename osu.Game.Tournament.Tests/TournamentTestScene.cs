// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Testing;
using osu.Game.Tests.Visual;

namespace osu.Game.Tournament.Tests
{
    public abstract class TournamentTestScene : OsuTestScene
    {
        protected override ITestSuiteTestRunner CreateRunner() => new TournamentTestSceneTestRunner();

        public class TournamentTestSceneTestRunner : TournamentGameBase, ITestSuiteTestRunner
        {
            private TestSuiteTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                // this has to be run here rather than LoadComplete because
                // TestScene.cs is checking the IsLoaded state (on another thread) and expects
                // the runner to be loaded at that point.
                Add(runner = new TestSuiteTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestSuite test) => runner.RunTestBlocking(test);
        }
    }
}
