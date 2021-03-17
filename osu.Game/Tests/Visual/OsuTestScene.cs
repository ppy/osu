// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [ExcludeFromDynamicCompile]
    public abstract class OsuTestScene : TestScene
    {
        protected Bindable<WorkingBeatmap> Beatmap { get; private set; }

        protected Bindable<RulesetInfo> Ruleset;

        protected Bindable<IReadOnlyList<Mod>> SelectedMods;

        protected new OsuScreenDependencies Dependencies { get; private set; }

        private DrawableRulesetDependencies rulesetDependencies;

        private Lazy<Storage> localStorage;
        protected Storage LocalStorage => localStorage.Value;

        private Lazy<DatabaseContextFactory> contextFactory;

        protected IAPIProvider API
        {
            get
            {
                if (UseOnlineAPI)
                    throw new InvalidOperationException($"Using the {nameof(OsuTestScene)} dummy API is not supported when {nameof(UseOnlineAPI)} is true");

                return dummyAPI;
            }
        }

        private DummyAPIAccess dummyAPI;

        protected DatabaseContextFactory ContextFactory => contextFactory.Value;

        /// <summary>
        /// Whether this test scene requires real-world API access.
        /// If true, this will bypass the local <see cref="DummyAPIAccess"/> and use the <see cref="OsuGameBase"/> provided one.
        /// </summary>
        protected virtual bool UseOnlineAPI => false;

        /// <summary>
        /// When running headless, there is an opportunity to use the host storage rather than creating a second isolated one.
        /// This is because the host is recycled per TestScene execution in headless at an nunit level.
        /// </summary>
        private Storage isolatedHostStorage;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            if (!UseFreshStoragePerRun)
                isolatedHostStorage = (parent.Get<GameHost>() as HeadlessGameHost)?.Storage;

            contextFactory = new Lazy<DatabaseContextFactory>(() =>
            {
                var factory = new DatabaseContextFactory(LocalStorage);

                // only reset the database if not using the host storage.
                // if we reset the host storage, it will delete global key bindings.
                if (isolatedHostStorage == null)
                    factory.ResetDatabase();

                using (var usage = factory.Get())
                    usage.Migrate();
                return factory;
            });

            RecycleLocalStorage();

            var baseDependencies = base.CreateChildDependencies(parent);

            var providedRuleset = CreateRuleset();
            if (providedRuleset != null)
                baseDependencies = rulesetDependencies = new DrawableRulesetDependencies(providedRuleset, baseDependencies);

            Dependencies = new OsuScreenDependencies(false, baseDependencies);

            Beatmap = Dependencies.Beatmap;
            Beatmap.SetDefault();

            Ruleset = Dependencies.Ruleset;
            Ruleset.SetDefault();

            SelectedMods = Dependencies.Mods;
            SelectedMods.SetDefault();

            if (!UseOnlineAPI)
            {
                dummyAPI = new DummyAPIAccess();
                Dependencies.CacheAs<IAPIProvider>(dummyAPI);
                Add(dummyAPI);
            }

            return Dependencies;
        }

        protected override Container<Drawable> Content => content ?? base.Content;

        private readonly Container content;

        protected OsuTestScene()
        {
            base.Content.Add(content = new DrawSizePreservingFillContainer());
        }

        protected virtual bool UseFreshStoragePerRun => false;

        public virtual void RecycleLocalStorage()
        {
            if (localStorage?.IsValueCreated == true)
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

            localStorage =
                new Lazy<Storage>(() => isolatedHostStorage ?? new NativeStorage(Path.Combine(RuntimeInfo.StartupDirectory, $"{GetType().Name}-{Guid.NewGuid()}")));
        }

        [Resolved]
        protected AudioManager Audio { get; private set; }

        [Resolved]
        protected MusicController MusicController { get; private set; }

        /// <summary>
        /// Creates the ruleset to be used for this test scene.
        /// </summary>
        /// <remarks>
        /// When testing against ruleset-specific components, this method must be overriden to their corresponding ruleset.
        /// </remarks>
        [CanBeNull]
        protected virtual Ruleset CreateRuleset() => null;

        protected virtual IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset);

        protected WorkingBeatmap CreateWorkingBeatmap(RulesetInfo ruleset) =>
            CreateWorkingBeatmap(CreateBeatmap(ruleset), null);

        protected virtual WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, Clock, Audio);

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Ruleset.Value = CreateRuleset()?.RulesetInfo ?? rulesets.AvailableRulesets.First();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            rulesetDependencies?.Dispose();

            if (MusicController?.TrackLoaded == true)
                MusicController.Stop();

            if (contextFactory?.IsValueCreated == true)
                contextFactory.Value.ResetDatabase();

            RecycleLocalStorage();
        }

        protected override ITestSceneTestRunner CreateRunner() => new OsuTestSceneTestRunner();

        public class ClockBackedTestWorkingBeatmap : TestWorkingBeatmap
        {
            private readonly Track track;

            private readonly TrackVirtualStore store;

            /// <summary>
            /// Create an instance which creates a <see cref="TestBeatmap"/> for the provided ruleset when requested.
            /// </summary>
            /// <param name="ruleset">The target ruleset.</param>
            /// <param name="referenceClock">A clock which should be used instead of a stopwatch for virtual time progression.</param>
            /// <param name="audio">Audio manager. Required if a reference clock isn't provided.</param>
            public ClockBackedTestWorkingBeatmap(RulesetInfo ruleset, IFrameBasedClock referenceClock, AudioManager audio)
                : this(new TestBeatmap(ruleset), null, referenceClock, audio)
            {
            }

            /// <summary>
            /// Create an instance which provides the <see cref="IBeatmap"/> when requested.
            /// </summary>
            /// <param name="beatmap">The beatmap</param>
            /// <param name="storyboard">The storyboard.</param>
            /// <param name="referenceClock">An optional clock which should be used instead of a stopwatch for virtual time progression.</param>
            /// <param name="audio">Audio manager. Required if a reference clock isn't provided.</param>
            public ClockBackedTestWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard, IFrameBasedClock referenceClock, AudioManager audio)
                : base(beatmap, storyboard, audio)
            {
                double trackLength = 60000;

                if (beatmap.HitObjects.Count > 0)
                    // add buffer after last hitobject to allow for final replay frames etc.
                    trackLength = Math.Max(trackLength, beatmap.HitObjects.Max(h => h.GetEndTime()) + 2000);

                if (referenceClock != null)
                {
                    store = new TrackVirtualStore(referenceClock);
                    audio.AddItem(store);
                    track = store.GetVirtual(trackLength);
                }
                else
                    track = audio?.Tracks.GetVirtual(trackLength);
            }

            ~ClockBackedTestWorkingBeatmap()
            {
                // Remove the track store from the audio manager
                store?.Dispose();
            }

            protected override Track GetBeatmapTrack() => track;

            public class TrackVirtualStore : AudioCollectionManager<Track>, ITrackStore
            {
                private readonly IFrameBasedClock referenceClock;

                public TrackVirtualStore(IFrameBasedClock referenceClock)
                {
                    this.referenceClock = referenceClock;
                }

                public Track Get(string name) => throw new NotImplementedException();

                public Task<Track> GetAsync(string name) => throw new NotImplementedException();

                public Stream GetStream(string name) => throw new NotImplementedException();

                public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

                public Track GetVirtual(double length = double.PositiveInfinity)
                {
                    var track = new TrackVirtualManual(referenceClock) { Length = length };
                    AddItem(track);
                    return track;
                }
            }

            /// <summary>
            /// A virtual track which tracks a reference clock.
            /// </summary>
            public class TrackVirtualManual : Track
            {
                private readonly IFrameBasedClock referenceClock;

                private bool running;

                public TrackVirtualManual(IFrameBasedClock referenceClock)
                {
                    this.referenceClock = referenceClock;
                    Length = double.PositiveInfinity;
                }

                public override bool Seek(double seek)
                {
                    accumulated = Math.Clamp(seek, 0, Length);
                    lastReferenceTime = null;

                    return accumulated == seek;
                }

                public override void Start()
                {
                    running = true;
                }

                public override void Reset()
                {
                    Seek(0);
                    base.Reset();
                }

                public override void Stop()
                {
                    if (running)
                    {
                        running = false;
                        lastReferenceTime = null;
                    }
                }

                public override bool IsRunning => running;

                private double? lastReferenceTime;

                private double accumulated;

                public override double CurrentTime => Math.Min(accumulated, Length);

                protected override void UpdateState()
                {
                    base.UpdateState();

                    if (running)
                    {
                        double refTime = referenceClock.CurrentTime;

                        double? lastRefTime = lastReferenceTime;

                        if (lastRefTime != null)
                            accumulated += (refTime - lastRefTime.Value) * Rate;

                        lastReferenceTime = refTime;
                    }

                    if (CurrentTime >= Length)
                    {
                        Stop();
                        RaiseCompleted();
                    }
                }
            }
        }

        public class OsuTestSceneTestRunner : OsuGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                // this has to be run here rather than LoadComplete because
                // TestScene.cs is checking the IsLoaded state (on another thread) and expects
                // the runner to be loaded at that point.
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
