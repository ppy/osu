// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuTestCase : TestCase
    {
        private readonly OsuTestBeatmap beatmap = new OsuTestBeatmap(new DummyWorkingBeatmap());
        protected BindableBeatmap Beatmap => beatmap;

        protected readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        protected DependencyContainer Dependencies { get; private set; }

        private readonly Lazy<Storage> localStorage;
        protected Storage LocalStorage => localStorage.Value;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            Dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            Dependencies.CacheAs<BindableBeatmap>(beatmap);
            Dependencies.CacheAs<IBindableBeatmap>(beatmap);

            Dependencies.CacheAs(Ruleset);
            Dependencies.CacheAs<IBindable<RulesetInfo>>(Ruleset);

            return Dependencies;
        }

        protected OsuTestCase()
        {
            localStorage = new Lazy<Storage>(() => new DesktopStorage($"{GetType().Name}-{Guid.NewGuid()}", null));
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audioManager, RulesetStore rulesets)
        {
            beatmap.SetAudioManager(audioManager);

            Ruleset.Value = rulesets.AvailableRulesets.First();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmap != null)
            {
                beatmap.Disabled = true;
                beatmap.Value.Track.Stop();
            }

            if (localStorage.IsValueCreated)
            {
                try
                {
                    localStorage.Value.DeleteDirectory(".");
                }
                catch
                {
                    // we don't really care if this fails; it will just leave folders lying around from test runs.
                }
            }
        }

        protected override ITestCaseTestRunner CreateRunner() => new OsuTestCaseTestRunner();

        public class OsuTestCaseTestRunner : OsuGameBase, ITestCaseTestRunner
        {
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

        private class OsuTestBeatmap : BindableBeatmap
        {
            public OsuTestBeatmap(WorkingBeatmap defaultValue)
                : base(defaultValue)
            {
            }

            public void SetAudioManager(AudioManager audioManager) => RegisterAudioManager(audioManager);

            public override BindableBeatmap GetBoundCopy()
            {
                var copy = new OsuTestBeatmap(Default);
                copy.BindTo(this);
                return copy;
            }
        }
    }
}
