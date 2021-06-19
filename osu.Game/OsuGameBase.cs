// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Online;
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
        /// The maximum volume at which audio tracks should playback. This can be set lower than 1 to create some head-room for sound effects.
        /// </summary>
        internal const double GLOBAL_TRACK_VOLUME_ADJUST = 0.8;

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
                return $@"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        protected OsuConfigManager LocalConfig { get; private set; }

        protected SessionStatics SessionStatics { get; private set; }

        protected BeatmapManager BeatmapManager { get; private set; }

        protected ScoreManager ScoreManager { get; private set; }

        protected SkinManager SkinManager { get; private set; }

        protected RulesetStore RulesetStore { get; private set; }

        protected KeyBindingStore KeyBindingStore { get; private set; }

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

        private FileStore fileStore;

        private SettingsStore settingsStore;

        private RulesetConfigCache rulesetConfigCache;

        private SpectatorClient spectatorClient;

        private MultiplayerClient multiplayerClient;

        private DatabaseContextFactory contextFactory;

        protected override Container<Drawable> Content => content;

        private Container content;

        private DependencyContainer dependencies;

        private Bindable<bool> fpsDisplayVisible;

        private readonly BindableNumber<double> globalTrackVolumeAdjust = new BindableNumber<double>(GLOBAL_TRACK_VOLUME_ADJUST);

        public OsuGameBase()
        {
            UseDevelopmentServer = DebugUtils.IsDebugBuild;
            Name = @"osu!lazer";
        }

        [BackgroundDependencyLoader]
        private void load()
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

            dependencies.Cache(contextFactory = new DatabaseContextFactory(Storage));

            dependencies.CacheAs(Storage);

            var largeStore = new LargeTextureStore(Host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
            largeStore.AddStore(Host.CreateTextureLoaderStore(new OnlineStore()));
            dependencies.Cache(largeStore);

            dependencies.CacheAs(this);
            dependencies.CacheAs(LocalConfig);

            AddFont(Resources, @"Fonts/osuFont");

            AddFont(Resources, @"Fonts/Torus-Regular");
            AddFont(Resources, @"Fonts/Torus-Light");
            AddFont(Resources, @"Fonts/Torus-SemiBold");
            AddFont(Resources, @"Fonts/Torus-Bold");

            AddFont(Resources, @"Fonts/Noto-Basic");
            AddFont(Resources, @"Fonts/Noto-Hangul");
            AddFont(Resources, @"Fonts/Noto-CJK-Basic");
            AddFont(Resources, @"Fonts/Noto-CJK-Compatibility");
            AddFont(Resources, @"Fonts/Noto-Thai");

            AddFont(Resources, @"Fonts/Venera-Light");
            AddFont(Resources, @"Fonts/Venera-Bold");
            AddFont(Resources, @"Fonts/Venera-Black");

            Audio.Samples.PlaybackConcurrency = SAMPLE_CONCURRENCY;

            runMigrations();

            dependencies.Cache(SkinManager = new SkinManager(Storage, contextFactory, Host, Resources, Audio));
            dependencies.CacheAs<ISkinSource>(SkinManager);

            // needs to be done here rather than inside SkinManager to ensure thread safety of CurrentSkinInfo.
            SkinManager.ItemRemoved.BindValueChanged(weakRemovedInfo =>
            {
                if (weakRemovedInfo.NewValue.TryGetTarget(out var removedInfo))
                {
                    Schedule(() =>
                    {
                        // check the removed skin is not the current user choice. if it is, switch back to default.
                        if (removedInfo.ID == SkinManager.CurrentSkinInfo.Value.ID)
                            SkinManager.CurrentSkinInfo.Value = SkinInfo.Default;
                    });
                }
            });

            EndpointConfiguration endpoints = UseDevelopmentServer ? (EndpointConfiguration)new DevelopmentEndpointConfiguration() : new ProductionEndpointConfiguration();

            MessageFormatter.WebsiteRootUrl = endpoints.WebsiteRootUrl;

            dependencies.CacheAs(API ??= new APIAccess(LocalConfig, endpoints, VersionHash));

            dependencies.CacheAs(spectatorClient = new OnlineSpectatorClient(endpoints));
            dependencies.CacheAs(multiplayerClient = new OnlineMultiplayerClient(endpoints));

            var defaultBeatmap = new DummyWorkingBeatmap(Audio, Textures);

            dependencies.Cache(RulesetStore = new RulesetStore(contextFactory, Storage));
            dependencies.Cache(fileStore = new FileStore(contextFactory, Storage));

            // ordering is important here to ensure foreign keys rules are not broken in ModelStore.Cleanup()
            dependencies.Cache(ScoreManager = new ScoreManager(RulesetStore, () => BeatmapManager, Storage, API, contextFactory, Host, () => difficultyCache, LocalConfig));
            dependencies.Cache(BeatmapManager = new BeatmapManager(Storage, contextFactory, RulesetStore, API, Audio, Resources, Host, defaultBeatmap, true));

            // this should likely be moved to ArchiveModelManager when another case appers where it is necessary
            // to have inter-dependent model managers. this could be obtained with an IHasForeign<T> interface to
            // allow lookups to be done on the child (ScoreManager in this case) to perform the cascading delete.
            List<ScoreInfo> getBeatmapScores(BeatmapSetInfo set)
            {
                var beatmapIds = BeatmapManager.QueryBeatmaps(b => b.BeatmapSetInfoID == set.ID).Select(b => b.ID).ToList();
                return ScoreManager.QueryScores(s => beatmapIds.Contains(s.Beatmap.ID)).ToList();
            }

            BeatmapManager.ItemRemoved.BindValueChanged(i =>
            {
                if (i.NewValue.TryGetTarget(out var item))
                    ScoreManager.Delete(getBeatmapScores(item), true);
            });

            BeatmapManager.ItemUpdated.BindValueChanged(i =>
            {
                if (i.NewValue.TryGetTarget(out var item))
                    ScoreManager.Undelete(getBeatmapScores(item), true);
            });

            dependencies.Cache(difficultyCache = new BeatmapDifficultyCache());
            AddInternal(difficultyCache);

            dependencies.Cache(userCache = new UserLookupCache());
            AddInternal(userCache);

            var scorePerformanceManager = new ScorePerformanceCache();
            dependencies.Cache(scorePerformanceManager);
            AddInternal(scorePerformanceManager);

            dependencies.Cache(KeyBindingStore = new KeyBindingStore(contextFactory, RulesetStore));
            dependencies.Cache(settingsStore = new SettingsStore(contextFactory));
            dependencies.Cache(rulesetConfigCache = new RulesetConfigCache(settingsStore));

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

            fileStore.Cleanup();

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

            KeyBindingStore.Register(globalBindings);
            dependencies.Cache(globalBindings);

            PreviewTrackManager previewTrackManager;
            dependencies.Cache(previewTrackManager = new PreviewTrackManager());
            Add(previewTrackManager);

            AddInternal(MusicController = new MusicController());
            dependencies.CacheAs(MusicController);

            Ruleset.BindValueChanged(onRulesetChanged);
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
            contextFactory.FlushConnections();
            (Storage as OsuStorage)?.Migrate(Host.GetStorage(path));
        }

        protected override UserInputManager CreateUserInputManager() => new OsuUserInputManager();

        protected virtual BatteryInfo CreateBatteryInfo() => null;

        protected virtual Container CreateScalingContainer() => new DrawSizePreservingFillContainer();

        protected override Storage CreateStorage(GameHost host, Storage defaultStorage) => new OsuStorage(host, defaultStorage);

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> r)
        {
            var dict = new Dictionary<ModType, IReadOnlyList<Mod>>();

            if (r.NewValue?.Available == true)
            {
                foreach (ModType type in Enum.GetValues(typeof(ModType)))
                    dict[type] = r.NewValue.CreateInstance().GetModsFor(type).ToList();
            }

            if (!SelectedMods.Disabled)
                SelectedMods.Value = Array.Empty<Mod>();

            AvailableMods.Value = dict;
        }

        private void runMigrations()
        {
            try
            {
                using (var db = contextFactory.GetForWrite(false))
                    db.Context.Migrate();
            }
            catch (Exception e)
            {
                Logger.Error(e.InnerException ?? e, "Migration failed! We'll be starting with a fresh database.", LoggingTarget.Database);

                // if we failed, let's delete the database and start fresh.
                // todo: we probably want a better (non-destructive) migrations/recovery process at a later point than this.
                contextFactory.ResetDatabase();

                Logger.Log("Database purged successfully.", LoggingTarget.Database);

                // only run once more, then hard bail.
                using (var db = contextFactory.GetForWrite(false))
                    db.Context.Migrate();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            RulesetStore?.Dispose();
            BeatmapManager?.Dispose();
            LocalConfig?.Dispose();

            contextFactory.FlushConnections();
        }
    }
}
