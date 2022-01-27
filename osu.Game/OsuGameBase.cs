// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Audio;
using osu.Game.Beatmaps;
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
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Resources;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Utils;
using RuntimeInfo = osu.Framework.RuntimeInfo;

namespace osu.Game
{
    /// <summary>
    /// The most basic <see cref="Game"/> that can be used to host osu! components and systems.
    /// Unlike <see cref="OsuGame"/>, this class will not load any kind of UI, allowing it to be used
    /// for provide dependencies to test cases without interfering with them.
    /// </summary>
    public partial class OsuGameBase : Framework.Game, ICanAcceptFiles
    {
        public const string CLIENT_STREAM_NAME = @"lazer";

        public const int SAMPLE_CONCURRENCY = 6;

        /// <summary>
        /// Length of debounce (in milliseconds) for commonly occuring sample playbacks that could stack.
        /// </summary>
        public const int SAMPLE_DEBOUNCE_TIME = 20;

        /// <summary>
        /// The maximum volume at which audio tracks should playback. This can be set lower than 1 to create some head-room for sound effects.
        /// </summary>
        private const double global_track_volume_adjust = 0.8;

        public bool UseDevelopmentServer { get; }

        public virtual Version AssemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

        /// <summary>
        /// MD5 representation of the game executable.
        /// </summary>
        public string VersionHash { get; private set; }

        public bool IsDeployedBuild => AssemblyVersion.Major > 0;

        public virtual string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebugBuild ? @"debug" : @"release");

                var version = AssemblyVersion;
                return $@"{version.Major}.{version.Minor}.{version.Build}-lazer";
            }
        }

        protected OsuConfigManager LocalConfig { get; private set; }

        protected SessionStatics SessionStatics { get; private set; }

        protected BeatmapManager BeatmapManager { get; private set; }

        protected BeatmapModelDownloader BeatmapDownloader { get; private set; }

        protected ScoreManager ScoreManager { get; private set; }

        protected ScoreModelDownloader ScoreDownloader { get; private set; }

        protected SkinManager SkinManager { get; private set; }

        protected RulesetStore RulesetStore { get; private set; }

        protected RealmKeyBindingStore KeyBindingStore { get; private set; }

        protected MenuCursorContainer MenuCursorContainer { get; private set; }

        protected MusicController MusicController { get; private set; }

        protected IAPIProvider API { get; set; }

        protected Storage Storage { get; set; }

        protected Bindable<WorkingBeatmap> Beatmap { get; private set; } // cached via load() method

        [Cached]
        [Cached(typeof(IBindable<RulesetInfo>))]
        protected readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

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
        public readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> AvailableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();

        private BeatmapDifficultyCache difficultyCache;

        private UserLookupCache userCache;
        private BeatmapLookupCache beatmapCache;

        private RulesetConfigCache rulesetConfigCache;

        private SpectatorClient spectatorClient;

        private MultiplayerClient multiplayerClient;

        private RealmAccess realm;

        protected override Container<Drawable> Content => content;

        private Container content;

        private DependencyContainer dependencies;

        private Bindable<bool> fpsDisplayVisible;

        private readonly BindableNumber<double> globalTrackVolumeAdjust = new BindableNumber<double>(global_track_volume_adjust);

        /// <summary>
        /// A legacy EF context factory if migration has not been performed to realm yet.
        /// </summary>
        protected DatabaseContextFactory EFContextFactory { get; private set; }

        public OsuGameBase()
        {
            UseDevelopmentServer = DebugUtils.IsDebugBuild;
            Name = @"osu!";
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

            if (Storage.Exists(DatabaseContextFactory.DATABASE_NAME))
                dependencies.Cache(EFContextFactory = new DatabaseContextFactory(Storage));

            dependencies.Cache(realm = new RealmAccess(Storage, "client", EFContextFactory));

            dependencies.Cache(RulesetStore = new RulesetStore(realm, Storage));
            dependencies.CacheAs<IRulesetStore>(RulesetStore);

            // Backup is taken here rather than in EFToRealmMigrator to avoid recycling realm contexts
            // after initial usages below. It can be moved once a direction is established for handling re-subscription.
            // See https://github.com/ppy/osu/pull/16547 for more discussion.
            if (EFContextFactory != null)
            {
                const string backup_folder = "backups";

                string migration = $"before_final_migration_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                EFContextFactory.CreateBackup(Path.Combine(backup_folder, $"client.{migration}.db"));
                realm.CreateBackup(Path.Combine(backup_folder, $"client.{migration}.realm"));

                using (var source = Storage.GetStream("collection.db"))
                {
                    if (source != null)
                    {
                        using (var destination = Storage.GetStream(Path.Combine(backup_folder, $"collection.{migration}.db"), FileAccess.Write, FileMode.CreateNew))
                            source.CopyTo(destination);
                    }
                }
            }

            dependencies.CacheAs(Storage);

            var largeStore = new LargeTextureStore(Host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
            largeStore.AddStore(Host.CreateTextureLoaderStore(new OnlineStore()));
            dependencies.Cache(largeStore);

            dependencies.CacheAs(this);
            dependencies.CacheAs(LocalConfig);

            InitialiseFonts();

            Audio.Samples.PlaybackConcurrency = SAMPLE_CONCURRENCY;

            dependencies.Cache(SkinManager = new SkinManager(Storage, realm, Host, Resources, Audio, Scheduler));
            dependencies.CacheAs<ISkinSource>(SkinManager);

            EndpointConfiguration endpoints = UseDevelopmentServer ? (EndpointConfiguration)new DevelopmentEndpointConfiguration() : new ProductionEndpointConfiguration();

            MessageFormatter.WebsiteRootUrl = endpoints.WebsiteRootUrl;

            dependencies.CacheAs(API ??= new APIAccess(LocalConfig, endpoints, VersionHash));

            dependencies.CacheAs(spectatorClient = new OnlineSpectatorClient(endpoints));
            dependencies.CacheAs(multiplayerClient = new OnlineMultiplayerClient(endpoints));

            var defaultBeatmap = new DummyWorkingBeatmap(Audio, Textures);

            // ordering is important here to ensure foreign keys rules are not broken in ModelStore.Cleanup()
            dependencies.Cache(ScoreManager = new ScoreManager(RulesetStore, () => BeatmapManager, Storage, realm, Scheduler, Host, () => difficultyCache, LocalConfig));
            dependencies.Cache(BeatmapManager = new BeatmapManager(Storage, realm, RulesetStore, API, Audio, Resources, Host, defaultBeatmap, performOnlineLookups: true));

            dependencies.Cache(BeatmapDownloader = new BeatmapModelDownloader(BeatmapManager, API));
            dependencies.Cache(ScoreDownloader = new ScoreModelDownloader(ScoreManager, API));

            dependencies.Cache(difficultyCache = new BeatmapDifficultyCache());
            AddInternal(difficultyCache);

            dependencies.Cache(userCache = new UserLookupCache());
            AddInternal(userCache);

            dependencies.Cache(beatmapCache = new BeatmapLookupCache());
            AddInternal(beatmapCache);

            var scorePerformanceManager = new ScorePerformanceCache();
            dependencies.Cache(scorePerformanceManager);
            AddInternal(scorePerformanceManager);

            dependencies.CacheAs<IRulesetConfigCache>(rulesetConfigCache = new RulesetConfigCache(realm, RulesetStore));

            var powerStatus = CreateBatteryInfo();
            if (powerStatus != null)
                dependencies.CacheAs(powerStatus);

            dependencies.Cache(SessionStatics = new SessionStatics());
            dependencies.Cache(new OsuColour());

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
                AddInternal(apiAccess);
            AddInternal(spectatorClient);
            AddInternal(multiplayerClient);

            AddInternal(rulesetConfigCache);

            GlobalActionContainer globalBindings;

            var mainContent = new Drawable[]
            {
                MenuCursorContainer = new MenuCursorContainer { RelativeSizeAxes = Axes.Both },
                // to avoid positional input being blocked by children, ensure the GlobalActionContainer is above everything.
                globalBindings = new GlobalActionContainer(this)
            };

            MenuCursorContainer.Child = content = new OsuTooltipContainer(MenuCursorContainer.Cursor) { RelativeSizeAxes = Axes.Both };

            base.Content.Add(CreateScalingContainer().WithChildren(mainContent));

            KeyBindingStore = new RealmKeyBindingStore(realm, keyCombinationProvider);
            KeyBindingStore.Register(globalBindings, RulesetStore.AvailableRulesets);

            dependencies.Cache(globalBindings);

            PreviewTrackManager previewTrackManager;
            dependencies.Cache(previewTrackManager = new PreviewTrackManager(BeatmapManager.BeatmapTrackStore));
            Add(previewTrackManager);

            AddInternal(MusicController = new MusicController());
            dependencies.CacheAs(MusicController);

            Ruleset.BindValueChanged(onRulesetChanged);
            Beatmap.BindValueChanged(onBeatmapChanged);
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // TODO: This is temporary until we reimplement the local FPS display.
            // It's just to allow end-users to access the framework FPS display without knowing the shortcut key.
            fpsDisplayVisible = LocalConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay);
            fpsDisplayVisible.ValueChanged += visible => { FrameStatistics.Value = visible.NewValue ? FrameStatisticsMode.Minimal : FrameStatisticsMode.None; };
            fpsDisplayVisible.TriggerChange();

            FrameStatistics.ValueChanged += e => fpsDisplayVisible.Value = e.NewValue != FrameStatisticsMode.None;
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
        }

        /// <summary>
        /// Use to programatically exit the game as if the user was triggering via alt-f4.
        /// Will keep persisting until an exit occurs (exit may be blocked multiple times).
        /// </summary>
        public void GracefullyExit()
        {
            if (!OnExiting())
                Exit();
            else
                Scheduler.AddDelayed(GracefullyExit, 2000);
        }

        public void Migrate(string path)
        {
            Logger.Log($@"Migrating osu! data from ""{Storage.GetFullPath(string.Empty)}"" to ""{path}""...");

            IDisposable realmBlocker = null;

            try
            {
                ManualResetEventSlim readyToRun = new ManualResetEventSlim();

                Scheduler.Add(() =>
                {
                    realmBlocker = realm.BlockAllOperations();

                    readyToRun.Set();
                }, false);

                readyToRun.Wait();

                (Storage as OsuStorage)?.Migrate(Host.GetStorage(path));
            }
            finally
            {
                realmBlocker?.Dispose();
            }

            Logger.Log(@"Migration complete!");
        }

        protected override UserInputManager CreateUserInputManager() => new OsuUserInputManager();

        protected virtual BatteryInfo CreateBatteryInfo() => null;

        protected virtual Container CreateScalingContainer() => new DrawSizePreservingFillContainer();

        protected override Storage CreateStorage(GameHost host, Storage defaultStorage) => new OsuStorage(host, defaultStorage);

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> valueChangedEvent)
        {
            if (IsLoaded && !ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException("Global beatmap bindable must be changed from update thread.");
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
                Ruleset.Value = r.OldValue?.Available == true ? r.OldValue : RulesetStore.AvailableRulesets.First();
                return;
            }

            var dict = new Dictionary<ModType, IReadOnlyList<Mod>>();

            foreach (ModType type in Enum.GetValues(typeof(ModType)))
            {
                dict[type] = instance.GetModsFor(type).ToList();
            }

            if (!SelectedMods.Disabled)
                SelectedMods.Value = Array.Empty<Mod>();

            AvailableMods.Value = dict;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            RulesetStore?.Dispose();
            BeatmapManager?.Dispose();
            LocalConfig?.Dispose();

            realm?.Dispose();
        }
    }
}
