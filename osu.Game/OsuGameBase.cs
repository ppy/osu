// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Configuration;
using osu.Framework.Development;
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
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics.Textures;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game
{
    /// <summary>
    /// The most basic <see cref="Game"/> that can be used to host osu! components and systems.
    /// Unlike <see cref="OsuGame"/>, this class will not load any kind of UI, allowing it to be used
    /// for provide dependencies to test cases without interfering with them.
    /// </summary>
    public class OsuGameBase : Framework.Game, ICanAcceptFiles
    {
        protected OsuConfigManager LocalConfig;

        protected BeatmapManager BeatmapManager;

        protected SkinManager SkinManager;

        protected RulesetStore RulesetStore;

        protected FileStore FileStore;

        protected ScoreStore ScoreStore;

        protected KeyBindingStore KeyBindingStore;

        protected SettingsStore SettingsStore;

        protected CursorOverrideContainer CursorOverrideContainer;

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        private Container content;

        protected override Container<Drawable> Content => content;

        public Bindable<WorkingBeatmap> Beatmap { get; private set; }

        private Bindable<bool> fpsDisplayVisible;

        protected AssemblyName AssemblyName => Assembly.GetEntryAssembly()?.GetName() ?? new AssemblyName { Version = new Version() };

        public bool IsDeployedBuild => AssemblyName.Version.Major > 0;

        public string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebug ? @"debug" : @"release");

                var assembly = AssemblyName;
                return $@"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}";
            }
        }

        public OsuGameBase()
        {
            Name = @"osu!lazer";
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        private DatabaseContextFactory contextFactory;

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(contextFactory = new DatabaseContextFactory(Host));

            dependencies.Cache(new LargeTextureStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures"))));

            dependencies.CacheAs(this);
            dependencies.Cache(LocalConfig);

            runMigrations();

            dependencies.Cache(SkinManager = new SkinManager(Host.Storage, contextFactory, Host, Audio));
            dependencies.CacheAs<ISkinSource>(SkinManager);

            var api = new APIAccess(LocalConfig);

            dependencies.Cache(api);
            dependencies.CacheAs<IAPIProvider>(api);

            dependencies.Cache(RulesetStore = new RulesetStore(contextFactory));
            dependencies.Cache(FileStore = new FileStore(contextFactory, Host.Storage));
            dependencies.Cache(BeatmapManager = new BeatmapManager(Host.Storage, contextFactory, RulesetStore, api, Audio, Host));
            dependencies.Cache(ScoreStore = new ScoreStore(Host.Storage, contextFactory, Host, BeatmapManager, RulesetStore));
            dependencies.Cache(KeyBindingStore = new KeyBindingStore(contextFactory, RulesetStore));
            dependencies.Cache(SettingsStore = new SettingsStore(contextFactory));
            dependencies.Cache(new OsuColour());

            fileImporters.Add(BeatmapManager);
            fileImporters.Add(ScoreStore);
            fileImporters.Add(SkinManager);

            //this completely overrides the framework default. will need to change once we make a proper FontStore.
            dependencies.Cache(Fonts = new FontStore { ScaleAdjust = 100 });

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/FontAwesome"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/osuFont"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Medium"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-MediumItalic"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Hangul"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Compatibility"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Regular"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-RegularItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-SemiBold"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-SemiBoldItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Bold"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-BoldItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Light"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-LightItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Black"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-BlackItalic"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Venera"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Venera-Light"));

            var defaultBeatmap = new DummyWorkingBeatmap(this);
            Beatmap = new NonNullableBindable<WorkingBeatmap>(defaultBeatmap);
            BeatmapManager.DefaultBeatmap = defaultBeatmap;

            // tracks play so loud our samples can't keep up.
            // this adds a global reduction of track volume for the time being.
            Audio.Track.AddAdjustment(AdjustableProperty.Volume, new BindableDouble(0.8));

            Beatmap.ValueChanged += b =>
            {
                var trackLoaded = lastBeatmap?.TrackLoaded ?? false;

                // compare to last beatmap as sometimes the two may share a track representation (optimisation, see WorkingBeatmap.TransferTo)
                if (!trackLoaded || lastBeatmap?.Track != b.Track)
                {
                    if (trackLoaded)
                    {
                        Debug.Assert(lastBeatmap != null);
                        Debug.Assert(lastBeatmap.Track != null);

                        lastBeatmap.RecycleTrack();
                    }

                    Audio.Track.AddItem(b.Track);
                }

                lastBeatmap = b;
            };

            FileStore.Cleanup();

            AddInternal(api);
        }

        private void runMigrations()
        {
            try
            {
                using (var db = contextFactory.GetForWrite())
                    db.Context.Migrate();
            }
            catch (MigrationFailedException e)
            {
                Logger.Error(e.InnerException ?? e, "Migration failed! We'll be starting with a fresh database.", LoggingTarget.Database);

                // if we failed, let's delete the database and start fresh.
                // todo: we probably want a better (non-destructive) migrations/recovery process at a later point than this.
                contextFactory.ResetDatabase();
                Logger.Log("Database purged successfully.", LoggingTarget.Database, LogLevel.Important);

                using (var db = contextFactory.GetForWrite())
                    db.Context.Migrate();
            }
        }

        private WorkingBeatmap lastBeatmap;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            GlobalActionContainer globalBinding;

            CursorOverrideContainer = new CursorOverrideContainer { RelativeSizeAxes = Axes.Both };
            CursorOverrideContainer.Child = globalBinding = new GlobalActionContainer(this)
            {
                RelativeSizeAxes = Axes.Both,
                Child = content = new OsuTooltipContainer(CursorOverrideContainer.Cursor) { RelativeSizeAxes = Axes.Both }
            };

            base.Content.Add(new DrawSizePreservingFillContainer { Child = CursorOverrideContainer });

            KeyBindingStore.Register(globalBinding);
            dependencies.Cache(globalBinding);

            // TODO: This is temporary until we reimplement the local FPS display.
            // It's just to allow end-users to access the framework FPS display without knowing the shortcut key.
            fpsDisplayVisible = LocalConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay);
            fpsDisplayVisible.ValueChanged += val => { FrameStatisticsMode = val ? FrameStatisticsMode.Minimal : FrameStatisticsMode.None; };
            fpsDisplayVisible.TriggerChange();
        }

        public override void SetHost(GameHost host)
        {
            if (LocalConfig == null)
                LocalConfig = new OsuConfigManager(host.Storage);
            base.SetHost(host);
        }

        private readonly List<ICanAcceptFiles> fileImporters = new List<ICanAcceptFiles>();

        public void Import(params string[] paths)
        {
            var extension = Path.GetExtension(paths.First());

            foreach (var importer in fileImporters)
                if (importer.HandledExtensions.Contains(extension)) importer.Import(paths);
        }

        public string[] HandledExtensions => fileImporters.SelectMany(i => i.HandledExtensions).ToArray();
    }
}
