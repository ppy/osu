// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Rulesets;

namespace osu.Game.Tests.Visual
{
    [ExcludeFromDynamicCompile]
    public abstract class OsuTestScene : TestScene
    {
        protected Bindable<WorkingBeatmap> Beatmap { get; private set; }

        protected Bindable<RulesetInfo> Ruleset;

        protected Bindable<IReadOnlyList<Mod>> SelectedMods;

        protected new OsuScreenDependencies Dependencies { get; private set; }

        protected IResourceStore<byte[]> Resources;

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

        /// <summary>
        /// Whether this test scene requires real-world API access.
        /// If true, this will bypass the local <see cref="DummyAPIAccess"/> and use the <see cref="OsuGameBase"/> provided one.
        /// </summary>
        protected virtual bool UseOnlineAPI => false;

        /// <summary>
        /// A database context factory to be used by test runs. Can be isolated and reset by setting <see cref="UseFreshStoragePerRun"/> to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// In interactive runs (ie. VisualTests) this will use the user's database if <see cref="UseFreshStoragePerRun"/> is not set to <c>true</c>.
        /// </remarks>
        protected DatabaseContextFactory ContextFactory => contextFactory.Value;

        private Lazy<DatabaseContextFactory> contextFactory;

        /// <summary>
        /// Whether a fresh storage should be initialised per test (method) run.
        /// </summary>
        /// <remarks>
        /// By default (ie. if not set to <c>true</c>):
        /// - in interactive runs, the user's storage will be used
        /// - in headless runs, a shared temporary storage will be used per test class.
        /// </remarks>
        protected virtual bool UseFreshStoragePerRun => false;

        /// <summary>
        /// A storage to be used by test runs. Can be isolated by setting <see cref="UseFreshStoragePerRun"/> to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// In interactive runs (ie. VisualTests) this will use the user's storage if <see cref="UseFreshStoragePerRun"/> is not set to <c>true</c>.
        /// </remarks>
        protected Storage LocalStorage => localStorage.Value;

        /// <summary>
        /// A cache for ruleset configurations to be used in this test scene.
        /// </summary>
        /// <remarks>
        /// This <see cref="IRulesetConfigCache"/> instance is provided to the children of this test scene via DI.
        /// It is only exposed so that test scenes themselves can access the ruleset config cache in a safe manner
        /// (<see cref="OsuTestScene"/>s cannot use DI themselves, as they will end up accessing the real cached instance from <see cref="OsuGameBase"/>).
        /// </remarks>
        protected IRulesetConfigCache RulesetConfigs { get; private set; }

        private Lazy<Storage> localStorage;

        private Storage headlessHostStorage;

        private DrawableRulesetDependencies rulesetDependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            headlessHostStorage = (parent.Get<GameHost>() as HeadlessGameHost)?.Storage;

            Resources = parent.Get<OsuGameBase>().Resources;

            contextFactory = new Lazy<DatabaseContextFactory>(() =>
            {
                var factory = new DatabaseContextFactory(LocalStorage);

                using (var usage = factory.Get())
                    usage.Migrate();
                return factory;
            });

            RecycleLocalStorage(false);

            var baseDependencies = base.CreateChildDependencies(parent);

            // to isolate ruleset configs in tests from the actual database and avoid state pollution problems,
            // as well as problems due to the implementation details of the "real" implementation (the configs only being available at `LoadComplete()`),
            // cache a test implementation of the ruleset config cache over the "real" one.
            var isolatedBaseDependencies = new DependencyContainer(baseDependencies);
            isolatedBaseDependencies.CacheAs(RulesetConfigs = new TestRulesetConfigCache());
            baseDependencies = isolatedBaseDependencies;

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
                base.Content.Add(dummyAPI);
            }

            return Dependencies;
        }

        protected override Container<Drawable> Content => content ?? base.Content;

        private readonly Container content;

        protected OsuTestScene()
        {
            base.Content.Add(content = new DrawSizePreservingFillContainer());
        }

        public virtual void RecycleLocalStorage(bool isDisposing)
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

            localStorage = new Lazy<Storage>(() =>
            {
                // When running headless, there is an opportunity to use the host storage rather than creating a second isolated one.
                // This is because the host is recycled per TestScene execution in headless at an nunit level.
                // Importantly, we can't use this optimisation when `UseFreshStoragePerRun` is true, as it doesn't reset per test method.
                if (!UseFreshStoragePerRun && headlessHostStorage != null)
                    return headlessHostStorage;

                return new TemporaryNativeStorage($"{GetType().Name}-{Guid.NewGuid()}");
            });
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

        /// <summary>
        /// Returns a sample API Beatmap with BeatmapSet populated.
        /// </summary>
        /// <param name="ruleset">The ruleset to create the sample model using. osu! ruleset will be used if not specified.</param>
        protected APIBeatmap CreateAPIBeatmap(RulesetInfo ruleset = null)
        {
            var beatmapSet = CreateAPIBeatmapSet(ruleset ?? Ruleset.Value);

            // Avoid circular reference.
            var beatmap = beatmapSet.Beatmaps.First();
            beatmapSet.Beatmaps = Array.Empty<APIBeatmap>();

            // Populate the set as that's generally what we expect from the API.
            beatmap.BeatmapSet = beatmapSet;

            return beatmap;
        }

        /// <summary>
        /// Returns a sample API BeatmapSet with beatmaps populated.
        /// </summary>
        /// <param name="ruleset">The ruleset to create the sample model using. osu! ruleset will be used if not specified.</param>
        protected APIBeatmapSet CreateAPIBeatmapSet(RulesetInfo ruleset = null)
        {
            var beatmap = CreateBeatmap(ruleset ?? Ruleset.Value).BeatmapInfo;

            Debug.Assert(beatmap.BeatmapSet != null);

            return new APIBeatmapSet
            {
                OnlineID = ((IBeatmapSetInfo)beatmap.BeatmapSet).OnlineID,
                Status = BeatmapOnlineStatus.Ranked,
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/163112/covers/cover.jpg",
                    Card = "https://assets.ppy.sh/beatmaps/163112/covers/card.jpg",
                    List = "https://assets.ppy.sh/beatmaps/163112/covers/list.jpg"
                },
                Title = beatmap.Metadata.Title,
                TitleUnicode = beatmap.Metadata.TitleUnicode,
                Artist = beatmap.Metadata.Artist,
                ArtistUnicode = beatmap.Metadata.ArtistUnicode,
                Author = new APIUser
                {
                    Username = beatmap.Metadata.Author.Username,
                    Id = beatmap.Metadata.Author.OnlineID
                },
                Source = beatmap.Metadata.Source,
                Tags = beatmap.Metadata.Tags,
                Beatmaps = new[]
                {
                    new APIBeatmap
                    {
                        OnlineID = ((IBeatmapInfo)beatmap).OnlineID,
                        OnlineBeatmapSetID = ((IBeatmapSetInfo)beatmap.BeatmapSet).OnlineID,
                        Status = beatmap.Status,
                        Checksum = beatmap.MD5Hash,
                        AuthorID = beatmap.Metadata.Author.OnlineID,
                        RulesetID = beatmap.RulesetID,
                        StarRating = beatmap.StarRating,
                        DifficultyName = beatmap.DifficultyName,
                    }
                }
            };
        }

        protected WorkingBeatmap CreateWorkingBeatmap(RulesetInfo ruleset) =>
            CreateWorkingBeatmap(CreateBeatmap(ruleset));

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

            RecycleLocalStorage(true);
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

                public Task<Track> GetAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

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
                        // `RaiseCompleted` is not called here to prevent transitioning to the next song.
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

            protected override void InitialiseFonts()
            {
                // skip fonts load as it's not required for testing purposes.
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
