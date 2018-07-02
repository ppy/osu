// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Configuration;
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

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            Dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

            Dependencies.CacheAs<BindableBeatmap>(beatmap);
            Dependencies.CacheAs<IBindableBeatmap>(beatmap);

            Dependencies.CacheAs(Ruleset);
            Dependencies.CacheAs<IBindable<RulesetInfo>>(Ruleset);

            return Dependencies;
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
