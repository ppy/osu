using System;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Processing;
using osu.Game.IPC;
using osu.Game.Online.API;
using osu.Game.Overlays;

namespace osu.Game
{
    public class OsuGameBase : BaseGame
    {
        internal OsuConfigManager Config;

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public APIAccess API;

        protected override Container<Drawable> Content => ratioContainer;

        private RatioAdjust ratioContainer;

        public CursorContainer Cursor;

        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public OsuGameBase()
        {
            AddInternal(ratioContainer = new RatioAdjust());

            Children = new Drawable[]
            {
                Cursor = new OsuCursorContainer { Depth = float.MaxValue }
            };

            Beatmap.ValueChanged += Beatmap_ValueChanged;
        }

        private void Beatmap_ValueChanged(object sender, EventArgs e)
        {
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);

            Dependencies.Cache(this);
            Dependencies.Cache<OsuConfigManager>();
            Dependencies.Cache(new BeatmapDatabase(Host.Storage, Host));

            OszArchiveReader.Register();

            //this completely overrides the framework default. will need to change once we make a proper FontStore.
            Fonts = new TextureStore() { ScaleAdjust = 0.01f };

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/FontAwesome"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/osuFont"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Regular"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-RegularItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-SemiBold"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-SemiBoldItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Bold"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-BoldItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Light"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-LightItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Medium"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-MediumItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Black"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-BlackItalic"));

            API = new APIAccess()
            {
                Username = Config.Get<string>(OsuConfig.Username),
                Password = Config.Get<string>(OsuConfig.Password),
                Token = Config.Get<string>(OsuConfig.Token)
            };
        }

        public override void SetHost(BasicGameHost host)
        {
            if (Config == null)
                Config = new OsuConfigManager(host.Storage);
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
            if (Config != null && API != null)
            {
                Config.Set(OsuConfig.Token, API.Token);
                Config.Save();
            }

            base.Dispose(isDisposing);
        }
    }
}
