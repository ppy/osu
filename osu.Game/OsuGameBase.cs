// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Processing;
using osu.Game.Online.API;
using SQLite.Net;
using osu.Framework.Graphics.Performance;
using osu.Game.Database;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;

namespace osu.Game
{
    public class OsuGameBase : Framework.Game, IOnlineComponent
    {
        protected OsuConfigManager LocalConfig;

        protected BeatmapManager BeatmapManager;

        protected RulesetStore RulesetStore;

        protected FileStore FileStore;

        protected ScoreStore ScoreStore;

        protected KeyBindingStore KeyBindingStore;

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public APIAccess API;

        private Container content;

        protected override Container<Drawable> Content => content;

        protected MenuCursor Cursor;

        public Bindable<WorkingBeatmap> Beatmap { get; private set; }

        private Bindable<bool> fpsDisplayVisible;

        protected AssemblyName AssemblyName => Assembly.GetEntryAssembly()?.GetName() ?? new AssemblyName { Version = new Version() };

        public bool IsDeployedBuild => AssemblyName.Version.Major > 0;

        public bool IsDebug
        {
            get
            {
                // ReSharper disable once RedundantAssignment
                bool isDebug = false;
                // Debug.Assert conditions are only evaluated in debug mode
                Debug.Assert(isDebug = true);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return isDebug;
            }
        }

        public string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (IsDebug ? @"debug" : @"release");

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

        private SQLiteConnection connection;

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(this);
            dependencies.Cache(LocalConfig);

            connection = Host.Storage.GetDatabase(@"client");

            connection.CreateTable<StoreVersion>();

            dependencies.Cache(API = new APIAccess
            {
                Username = LocalConfig.Get<string>(OsuSetting.Username),
                Token = LocalConfig.Get<string>(OsuSetting.Token)
            });

            dependencies.Cache(RulesetStore = new RulesetStore(connection));
            dependencies.Cache(FileStore = new FileStore(connection, Host.Storage));
            dependencies.Cache(BeatmapManager = new BeatmapManager(Host.Storage, FileStore, connection, RulesetStore, API, Host));
            dependencies.Cache(ScoreStore = new ScoreStore(Host.Storage, connection, Host, BeatmapManager, RulesetStore));
            dependencies.Cache(KeyBindingStore = new KeyBindingStore(connection, RulesetStore));
            dependencies.Cache(new OsuColour());

            //this completely overrides the framework default. will need to change once we make a proper FontStore.
            dependencies.Cache(Fonts = new FontStore { ScaleAdjust = 100 }, true);

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

                        lastBeatmap.DisposeTrack();
                    }

                    Audio.Track.AddItem(b.Track);
                }

                lastBeatmap = b;
            };

            API.Register(this);
        }

        private WorkingBeatmap lastBeatmap;

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    LocalConfig.Set(OsuSetting.Username, LocalConfig.Get<bool>(OsuSetting.SaveUsername) ? API.Username : string.Empty);
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            GlobalKeyBindingInputManager globalBinding;

            base.Content.Add(new RatioAdjust
            {
                Children = new Drawable[]
                {
                    Cursor = new MenuCursor(),
                    globalBinding = new GlobalKeyBindingInputManager(this)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new OsuTooltipContainer(Cursor)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = content = new OsuContextMenuContainer { RelativeSizeAxes = Axes.Both },
                        }
                    }
                }
            });

            KeyBindingStore.Register(globalBinding);
            dependencies.Cache(globalBinding);

            // TODO: This is temporary until we reimplement the local FPS display.
            // It's just to allow end-users to access the framework FPS display without knowing the shortcut key.
            fpsDisplayVisible = LocalConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay);
            fpsDisplayVisible.ValueChanged += val =>
            {
                FrameStatisticsMode = val ? FrameStatisticsMode.Minimal : FrameStatisticsMode.None;
            };
            fpsDisplayVisible.TriggerChange();
        }

        public override void SetHost(GameHost host)
        {
            if (LocalConfig == null)
                LocalConfig = new OsuConfigManager(host.Storage);
            base.SetHost(host);
        }

        protected override void Update()
        {
            base.Update();
            API.Update();
        }

        protected override void Dispose(bool isDisposing)
        {
            //refresh token may have changed.
            if (LocalConfig != null && API != null)
            {
                LocalConfig.Set(OsuSetting.Token, LocalConfig.Get<bool>(OsuSetting.SavePassword) ? API.Token : string.Empty);
                LocalConfig.Save();
            }

            connection.Dispose();

            base.Dispose(isDisposing);
        }
    }
}
