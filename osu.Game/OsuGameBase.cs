// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Input.Handlers.Midi;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Input.Handlers.Touch;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Online.Metadata;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Solo;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Resources;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Utils;
using File = System.IO.File;
using RuntimeInfo = osu.Framework.RuntimeInfo;

namespace osu.Game
{
    /// <summary>
    /// The most basic <see cref="Game"/> that can be used to host osu! components and systems.
    /// Unlike <see cref="OsuGame"/>, this class will not load any kind of UI, allowing it to be used
    /// for provide dependencies to test cases without interfering with them.
    /// </summary>
    [Cached(typeof(OsuGameBase))]
    public partial class OsuGameBase : Framework.Game, ICanAcceptFiles, IBeatSyncProvider
    {
        public static readonly string[] VIDEO_EXTENSIONS = { ".mp4", ".mov", ".avi", ".flv" };

        public const string OSU_PROTOCOL = "osu://";

        public const string CLIENT_STREAM_NAME = @"lazer";

        /// <summary>
        /// The filename of the main client database.
        /// </summary>
        public const string CLIENT_DATABASE_FILENAME = @"client.realm";

        public const int SAMPLE_CONCURRENCY = 6;

        public const double SFX_STEREO_STRENGTH = 0.75;

        /// <summary>
        /// Length of debounce (in milliseconds) for commonly occuring sample playbacks that could stack.
        /// </summary>
        public const int SAMPLE_DEBOUNCE_TIME = 20;

        /// <summary>
        /// The maximum volume at which audio tracks should playback. This can be set lower than 1 to create some head-room for sound effects.
        /// </summary>
        private const double global_track_volume_adjust = 0.8;

        public virtual bool UseDevelopmentServer => DebugUtils.IsDebugBuild;

        public virtual EndpointConfiguration CreateEndpoints() =>
            UseDevelopmentServer ? new DevelopmentEndpointConfiguration() : new ExperimentalEndpointConfiguration();

        public virtual Version AssemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

        /// <summary>
        /// MD5 representation of the game executable.
        /// </summary>
        public string VersionHash { get; private set; }

        public bool IsDeployedBuild => AssemblyVersion.Major > 0;

        internal const string BUILD_SUFFIX = "lazer";

        public virtual string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebugBuild ? @"debug" : @"release");

                var version = AssemblyVersion;
                return $@"{version.Major}.{version.Minor}.{version.Build}-{BUILD_SUFFIX}";
            }
        }

        /// <summary>
        /// The <see cref="Edges"/> that the game should be drawn over at a top level.
        /// Defaults to <see cref="Edges.None"/>.
        /// </summary>
        protected virtual Edges SafeAreaOverrideEdges => Edges.None;

        protected OsuConfigManager LocalConfig { get; private set; }

        protected SessionStatics SessionStatics { get; private set; }

        protected OsuColour Colours { get; private set; }

        protected BeatmapManager BeatmapManager { get; private set; }

        protected BeatmapModelDownloader BeatmapDownloader { get; private set; }

        protected ScoreManager ScoreManager { get; private set; }

        protected ScoreModelDownloader ScoreDownloader { get; private set; }

        protected SkinManager SkinManager { get; private set; }

        protected RealmRulesetStore RulesetStore { get; private set; }

        protected RealmKeyBindingStore KeyBindingStore { get; private set; }

        protected GlobalCursorDisplay GlobalCursorDisplay { get; private set; }

        protected MusicController MusicController { get; private set; }

        protected IAPIProvider API { get; set; }

        protected Storage Storage { get; set; }

        protected Bindable<WorkingBeatmap> Beatmap { get; private set; } // cached via load() method

        /// <summary>
        /// The current ruleset selection for the local user.
        /// </summary>
        [Cached]
        [Cached(typeof(IBindable<RulesetInfo>))]
        protected internal readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        /// <summary>
        /// The current mod selection for the local user.
        /// </summary>
        /// <remarks>
        /// If a mod select overlay is present, mod instances set to this value are not guaranteed to remain as the provided instance and will be overwritten by a copy.
        /// In such a case, changes to settings of a mod will *not* propagate after a mod is added to this collection.
        /// As such, all settings should be finalised before adding a mod to this collection.
        /// </remarks>
        [Cached]
        [Cached(typeof(IBindable<IReadOnlyList<Mod>>))]
        protected readonly Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// Mods available for the current <see cref="Ruleset"/>.
        /// </summary>
        public readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> AvailableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>(new Dictionary<ModType, IReadOnlyList<Mod>>());

        private BeatmapDifficultyCache difficultyCache;
        private BeatmapUpdater beatmapUpdater;

        private UserLookupCache userCache;
        private BeatmapLookupCache beatmapCache;

        private RulesetConfigCache rulesetConfigCache;

        protected SpectatorClient SpectatorClient { get; private set; }

        protected MultiplayerClient MultiplayerClient { get; private set; }

        private MetadataClient metadataClient;
        private SoloStatisticsWatcher soloStatisticsWatcher;

        private RealmAccess realm;

        protected SafeAreaContainer SafeAreaContainer { get; private set; }

        /// <summary>
        /// For now, this is used as a source specifically for beat synced components.
        /// Going forward, it could potentially be used as the single source-of-truth for beatmap timing.
        /// </summary>
        private readonly FramedBeatmapClock beatmapClock = new FramedBeatmapClock(true);

        protected override Container<Drawable> Content => content;

        private Container content;

        private DependencyContainer dependencies;

        private readonly BindableNumber<double> globalTrackVolumeAdjust = new BindableNumber<double>(global_track_volume_adjust);

        /// <summary>
        /// Number of unhandled exceptions to allow before aborting execution.
        /// </summary>
        /// <remarks>
        /// When an unhandled exception is encountered, an internal count will be decremented.
        /// If the count hits zero, the game will crash.
        /// Each second, the count is incremented until reaching the value specified.
        /// </remarks>
        protected virtual int UnhandledExceptionsBeforeCrash => DebugUtils.IsDebugBuild ? 0 : 1;

        public OsuGameBase()
        {
            Name = @"osu!";

#if DEBUG
            Name += " (development)";
#endif

            allowableExceptions = UnhandledExceptionsBeforeCrash;
        }

        [BackgroundDependencyLoader]
        private void load(ReadableKeyCombinationProvider keyCombinationProvider)
        {
            try
            {
                using (var str = File.OpenRead(typeof(OsuGameBase).Assembly.Location))
                    VersionHash = str.ComputeMD5Hash();
            }
            catch
            {
                // special case for android builds, which can't read DLLs from a packed apk.
                // should eventually be handled in a better way.
                VersionHash = $"{Version}-{RuntimeInfo.OS}".ComputeMD5Hash();
            }

            Resources.AddStore(new DllResourceStore(OsuResources.ResourceAssembly));

            dependencies.Cache(realm = new RealmAccess(Storage, CLIENT_DATABASE_FILENAME, Host.UpdateThread));

            dependencies.CacheAs<RulesetStore>(RulesetStore = new RealmRulesetStore(realm, Storage));
            dependencies.CacheAs<IRulesetStore>(RulesetStore);

            Decoder.RegisterDependencies(RulesetStore);

            dependencies.CacheAs(Storage);

            var largeStore = new LargeTextureStore(Host.Renderer, Host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
            largeStore.AddTextureSource(Host.CreateTextureLoaderStore(new OnlineStore()));
            dependencies.Cache(largeStore);

            dependencies.CacheAs(LocalConfig);
            dependencies.CacheAs<IGameplaySettings>(LocalConfig);

            InitialiseFonts();

            addFilesWarning();

            Audio.Samples.PlaybackConcurrency = SAMPLE_CONCURRENCY;

            dependencies.Cache(SkinManager = new SkinManager(Storage, realm, Host, Resources, Audio, Scheduler));
            dependencies.CacheAs<ISkinSource>(SkinManager);

            EndpointConfiguration endpoints = CreateEndpoints();

            MessageFormatter.WebsiteRootUrl = endpoints.WebsiteRootUrl;

            dependencies.CacheAs(API ??= new APIAccess(LocalConfig, endpoints, VersionHash));

            var defaultBeatmap = new DummyWorkingBeatmap(Audio, Textures);

            dependencies.Cache(difficultyCache = new BeatmapDifficultyCache());

            // ordering is important here to ensure foreign keys rules are not broken in ModelStore.Cleanup()
            dependencies.Cache(ScoreManager = new ScoreManager(RulesetStore, () => BeatmapManager, Storage, realm, API, LocalConfig));

            dependencies.Cache(BeatmapManager = new BeatmapManager(Storage, realm, API, Audio, Resources, Host, defaultBeatmap, difficultyCache, performOnlineLookups: true));

            dependencies.Cache(BeatmapDownloader = new BeatmapModelDownloader(BeatmapManager, API));
            dependencies.Cache(ScoreDownloader = new ScoreModelDownloader(ScoreManager, API));

            // Add after all the above cache operations as it depends on them.
            base.Content.Add(difficultyCache);

            // TODO: OsuGame or OsuGameBase?
            dependencies.CacheAs(beatmapUpdater = new BeatmapUpdater(BeatmapManager, difficultyCache, API, Storage));
            dependencies.CacheAs(SpectatorClient = new OnlineSpectatorClient(endpoints));
            dependencies.CacheAs(MultiplayerClient = new OnlineMultiplayerClient(endpoints));
            dependencies.CacheAs(metadataClient = new OnlineMetadataClient(endpoints));
            dependencies.CacheAs(soloStatisticsWatcher = new SoloStatisticsWatcher());

            base.Content.Add(new BeatmapOnlineChangeIngest(beatmapUpdater, realm, metadataClient));

            BeatmapManager.ProcessBeatmap = args => beatmapUpdater.Process(args.beatmapSet, !args.isBatch);

            dependencies.Cache(userCache = new UserLookupCache());
            base.Content.Add(userCache);

            dependencies.Cache(beatmapCache = new BeatmapLookupCache());
            base.Content.Add(beatmapCache);

            var scorePerformanceManager = new ScorePerformanceCache();
            dependencies.Cache(scorePerformanceManager);
            base.Content.Add(scorePerformanceManager);

            dependencies.CacheAs<IRulesetConfigCache>(rulesetConfigCache = new RulesetConfigCache(realm, RulesetStore));

            var powerStatus = CreateBatteryInfo();
            if (powerStatus != null)
                dependencies.CacheAs(powerStatus);

            dependencies.Cache(SessionStatics = new SessionStatics());
            dependencies.Cache(Colours = new OsuColour());

            RegisterImportHandler(BeatmapManager);
            RegisterImportHandler(ScoreManager);
            RegisterImportHandler(SkinManager);

            // drop track volume game-wide to leave some head-room for UI effects / samples.
            // this means that for the time being, gameplay sample playback is louder relative to the audio track, compared to stable.
            // we may want to revisit this if users notice or complain about the difference (consider this a bit of a trial).
            Audio.Tracks.AddAdjustment(AdjustableProperty.Volume, globalTrackVolumeAdjust);

            Beatmap = new NonNullableBindable<WorkingBeatmap>(defaultBeatmap);

            dependencies.CacheAs<IBindable<WorkingBeatmap>>(Beatmap);
            dependencies.CacheAs(Beatmap);

            // add api components to hierarchy.
            if (API is APIAccess apiAccess)
                base.Content.Add(apiAccess);

            base.Content.Add(SpectatorClient);
            base.Content.Add(MultiplayerClient);
            base.Content.Add(metadataClient);
            base.Content.Add(soloStatisticsWatcher);

            base.Content.Add(rulesetConfigCache);

            PreviewTrackManager previewTrackManager;
            dependencies.Cache(previewTrackManager = new PreviewTrackManager(BeatmapManager.BeatmapTrackStore));
            base.Content.Add(previewTrackManager);

            base.Content.Add(MusicController = new MusicController());
            dependencies.CacheAs(MusicController);

            MusicController.TrackChanged += onTrackChanged;
            base.Content.Add(beatmapClock);

            GlobalActionContainer globalBindings;

            base.Content.Add(SafeAreaContainer = new SafeAreaContainer
            {
                SafeAreaOverrideEdges = SafeAreaOverrideEdges,
                RelativeSizeAxes = Axes.Both,
                Child = CreateScalingContainer().WithChildren(new Drawable[]
                {
                    (GlobalCursorDisplay = new GlobalCursorDisplay
                    {
                        RelativeSizeAxes = Axes.Both
                    }).WithChild(content = new OsuTooltipContainer(GlobalCursorDisplay.MenuCursor)
                    {
                        RelativeSizeAxes = Axes.Both
                    }),
                    // to avoid positional input being blocked by children, ensure the GlobalActionContainer is above everything.
                    globalBindings = new GlobalActionContainer(this)
                })
            });

            KeyBindingStore = new RealmKeyBindingStore(realm, keyCombinationProvider);
            KeyBindingStore.Register(globalBindings, RulesetStore.AvailableRulesets);

            dependencies.Cache(globalBindings);

            Ruleset.BindValueChanged(onRulesetChanged);
            Beatmap.BindValueChanged(onBeatmapChanged);
        }

        private void addFilesWarning()
        {
            var realmStore = new RealmFileStore(realm, Storage);

            const string filename = "IMPORTANT READ ME.txt";

            if (!realmStore.Storage.Exists(filename))
            {
                using (var stream = realmStore.Storage.CreateFileSafely(filename))
                using (var textWriter = new StreamWriter(stream))
                {
                    textWriter.WriteLine(@"This folder contains all your user files (beatmaps, skins, replays etc.)");
                    textWriter.WriteLine(@"Please do not touch or delete this folder!!");
                    textWriter.WriteLine();
                    textWriter.WriteLine(@"If you are really looking to completely delete user data, please delete");
                    textWriter.WriteLine(@"the parent folder including all other files and directories");
                    textWriter.WriteLine();
                    textWriter.WriteLine(@"For more information on how these files are organised,");
                    textWriter.WriteLine(@"see https://github.com/ppy/osu/wiki/User-file-storage");
                }
            }
        }

        private void onTrackChanged(WorkingBeatmap beatmap, TrackChangeDirection direction)
        {
            // FramedBeatmapClock uses a decoupled clock internally which will mutate the source if it is an `IAdjustableClock`.
            // We don't want this for now, as the intention of beatmapClock is to be a read-only source for beat sync components.
            //
            // Encapsulating in a FramedClock will avoid any mutations.
            var framedClock = new FramedClock(beatmap.Track);

            beatmapClock.ChangeSource(framedClock);
        }

        protected virtual void InitialiseFonts()
        {
            AddFont(Resources, @"Fonts/osuFont");

            AddFont(Resources, @"Fonts/Torus/Torus-Regular");
            AddFont(Resources, @"Fonts/Torus/Torus-Light");
            AddFont(Resources, @"Fonts/Torus/Torus-SemiBold");
            AddFont(Resources, @"Fonts/Torus/Torus-Bold");

            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-Regular");
            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-Light");
            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-SemiBold");
            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-Bold");

            AddFont(Resources, @"Fonts/Inter/Inter-Regular");
            AddFont(Resources, @"Fonts/Inter/Inter-RegularItalic");
            AddFont(Resources, @"Fonts/Inter/Inter-Light");
            AddFont(Resources, @"Fonts/Inter/Inter-LightItalic");
            AddFont(Resources, @"Fonts/Inter/Inter-SemiBold");
            AddFont(Resources, @"Fonts/Inter/Inter-SemiBoldItalic");
            AddFont(Resources, @"Fonts/Inter/Inter-Bold");
            AddFont(Resources, @"Fonts/Inter/Inter-BoldItalic");

            AddFont(Resources, @"Fonts/Noto/Noto-Basic");
            AddFont(Resources, @"Fonts/Noto/Noto-Hangul");
            AddFont(Resources, @"Fonts/Noto/Noto-CJK-Basic");
            AddFont(Resources, @"Fonts/Noto/Noto-CJK-Compatibility");
            AddFont(Resources, @"Fonts/Noto/Noto-Thai");

            AddFont(Resources, @"Fonts/Venera/Venera-Light");
            AddFont(Resources, @"Fonts/Venera/Venera-Bold");
            AddFont(Resources, @"Fonts/Venera/Venera-Black");
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            // may be non-null for certain tests
            Storage ??= host.Storage;

            LocalConfig ??= UseDevelopmentServer
                ? new DevelopmentOsuConfigManager(Storage)
                : new OsuConfigManager(Storage);

            host.ExceptionThrown += onExceptionThrown;
        }

        /// <summary>
        /// Use to programatically exit the game as if the user was triggering via alt-f4.
        /// By default, will keep persisting until an exit occurs (exit may be blocked multiple times).
        /// May be interrupted (see <see cref="OsuGame"/>'s override).
        /// </summary>
        public virtual void AttemptExit()
        {
            if (!OnExiting())
                Exit();
            else
                Scheduler.AddDelayed(AttemptExit, 2000);
        }

        public bool Migrate(string path)
        {
            Logger.Log($@"Migrating osu! data from ""{Storage.GetFullPath(string.Empty)}"" to ""{path}""...");

            IDisposable realmBlocker = null;

            try
            {
                ManualResetEventSlim readyToRun = new ManualResetEventSlim();

                Scheduler.Add(() =>
                {
                    realmBlocker = realm.BlockAllOperations("migration");

                    readyToRun.Set();
                }, false);

                if (!readyToRun.Wait(30000))
                    throw new TimeoutException("Attempting to block for migration took too long.");

                bool? cleanupSucceded = (Storage as OsuStorage)?.Migrate(Host.GetStorage(path));

                Logger.Log(@"Migration complete!");
                return cleanupSucceded != false;
            }
            finally
            {
                realmBlocker?.Dispose();
            }
        }

        protected override UserInputManager CreateUserInputManager() => new OsuUserInputManager();

        protected virtual BatteryInfo CreateBatteryInfo() => null;

        protected virtual Container CreateScalingContainer() => new DrawSizePreservingFillContainer();

        protected override Storage CreateStorage(GameHost host, Storage defaultStorage) => new OsuStorage(host, defaultStorage);

        /// <summary>
        /// Creates an input settings subsection for an <see cref="InputHandler"/>.
        /// </summary>
        /// <remarks>Should be overriden per-platform to provide settings for platform-specific handlers.</remarks>
        public virtual SettingsSubsection CreateSettingsSubsectionFor(InputHandler handler)
        {
            // One would think that this could be moved to the `OsuGameDesktop` class, but doing so means that
            // OsuGameTestScenes will not show any input options (as they are based on OsuGame not OsuGameDesktop).
            //
            // This in turn makes it hard for ruleset creators to adjust input settings while testing their ruleset
            // within the test browser interface.
            if (RuntimeInfo.IsDesktop)
            {
                switch (handler)
                {
                    case ITabletHandler th:
                        return new TabletSettings(th);

                    case MouseHandler mh:
                        return new MouseSettings(mh);

                    case JoystickHandler jh:
                        return new JoystickSettings(jh);

                    case TouchHandler th:
                        return new TouchSettings(th);
                }
            }

            switch (handler)
            {
                case MidiHandler:
                    return new InputSection.HandlerSection(handler);

                // return null for handlers that shouldn't have settings.
                default:
                    return null;
            }
        }

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            if (IsLoaded && !ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException("Global beatmap bindable must be changed from update thread.");

            Logger.Log($"Game-wide working beatmap updated to {beatmap.NewValue}");
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> r)
        {
            if (IsLoaded && !ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException("Global ruleset bindable must be changed from update thread.");

            Ruleset instance = null;

            try
            {
                if (r.NewValue?.Available == true)
                {
                    instance = r.NewValue.CreateInstance();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Ruleset load failed and has been rolled back");
            }

            if (instance == null)
            {
                // reject the change if the ruleset is not available.
                revertRulesetChange();
                return;
            }

            var dict = new Dictionary<ModType, IReadOnlyList<Mod>>();

            try
            {
                foreach (ModType type in Enum.GetValues<ModType>())
                {
                    dict[type] = instance.GetModsFor(type)
                                         // Rulesets should never return null mods, but let's be defensive just in case.
                                         // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                         .Where(mod => mod != null)
                                         .ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Could not load mods for \"{instance.RulesetInfo.Name}\" ruleset. Current ruleset has been rolled back.");
                revertRulesetChange();
                return;
            }

            var previouslySelectedMods = SelectedMods.Value.ToArray();

            if (!SelectedMods.Disabled)
                SelectedMods.Value = Array.Empty<Mod>();

            AvailableMods.Value = dict;

            if (!SelectedMods.Disabled)
                SelectedMods.Value = previouslySelectedMods.Select(m => instance.CreateModFromAcronym(m.Acronym)).Where(m => m != null).ToArray();

            void revertRulesetChange() => Ruleset.Value = r.OldValue?.Available == true ? r.OldValue : RulesetStore.AvailableRulesets.First();
        }

        private int allowableExceptions;

        /// <summary>
        /// Allows a maximum of one unhandled exception, per second of execution.
        /// </summary>
        private bool onExceptionThrown(Exception _)
        {
            bool continueExecution = Interlocked.Decrement(ref allowableExceptions) >= 0;

            Logger.Log($"Unhandled exception has been {(continueExecution ? $"allowed with {allowableExceptions} more allowable exceptions" : "denied")} .");

            // restore the stock of allowable exceptions after a short delay.
            Task.Delay(1000).ContinueWith(_ => Interlocked.Increment(ref allowableExceptions));

            return continueExecution;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            RulesetStore?.Dispose();
            LocalConfig?.Dispose();

            beatmapUpdater?.Dispose();

            realm?.Dispose();

            if (Host != null)
                Host.ExceptionThrown -= onExceptionThrown;
        }

        ControlPointInfo IBeatSyncProvider.ControlPoints => Beatmap.Value.BeatmapLoaded ? Beatmap.Value.Beatmap.ControlPointInfo : null;
        IClock IBeatSyncProvider.Clock => beatmapClock;
        ChannelAmplitudes IHasAmplitudes.CurrentAmplitudes => Beatmap.Value.TrackLoaded ? Beatmap.Value.Track.CurrentAmplitudes : ChannelAmplitudes.Empty;
    }
}
