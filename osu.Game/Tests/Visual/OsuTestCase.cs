// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        protected readonly GameBeatmap Beatmap = new GameBeatmap(new DummyWorkingBeatmap());

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.CacheAs<IGameBeatmap>(Beatmap);
            dependencies.Cache(Beatmap);
        }

        protected override ITestCaseTestRunner CreateRunner() => new OsuTestCaseTestRunner();

        public class OsuTestCaseTestRunner : OsuGameBase, ITestCaseTestRunner
        {
            protected override string MainResourceFile => File.Exists(base.MainResourceFile) ? base.MainResourceFile : Assembly.GetExecutingAssembly().Location;

            private TestCaseTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                // this has to be run here rather than LoadComplete because
                // TestCase.cs is checking the IsLoaded state (on another thread) and expects
                // the runner to be loaded at that point.
                Add(runner = new TestCaseTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestCase test) => runner.RunTestBlocking(test);
        }
    }
}
