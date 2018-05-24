// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        protected GameBeatmap Beatmap { get; private set; }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            // The beatmap is constructed here rather than load() because our children get dependencies injected before our load() runs
            Beatmap = new GameBeatmap(new DummyWorkingBeatmap(), parent.Get<AudioManager>());

            dependencies = new DependencyContainer(parent);

            dependencies.CacheAs<IGameBeatmap>(Beatmap);
            dependencies.Cache(Beatmap);

            return dependencies;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (Beatmap != null)
            {
                Beatmap.Disabled = true;
                Beatmap.Value.Track.Stop();
            }
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
