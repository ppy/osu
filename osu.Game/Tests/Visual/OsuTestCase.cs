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
        private readonly OsuTestBeatmap beatmap = new OsuTestBeatmap(new DummyWorkingBeatmap());
        protected GameBeatmap Beatmap => beatmap;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

            dependencies.CacheAs<GameBeatmap>(beatmap);
            dependencies.CacheAs<IGameBeatmap>(beatmap);

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audioManager)
        {
            beatmap.SetAudioManager(audioManager);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmap != null)
            {
                beatmap.Disabled = true;
                beatmap.Value.Track.Stop();
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

        private class OsuTestBeatmap : GameBeatmap
        {
            public OsuTestBeatmap(WorkingBeatmap defaultValue)
                : base(defaultValue)
            {
            }

            public void SetAudioManager(AudioManager audioManager) => RegisterAudioManager(audioManager);

            public override GameBeatmap GetBoundCopy()
            {
                var copy = new OsuTestBeatmap(Default);
                copy.BindTo(this);
                return copy;
            }
        }
    }
}
