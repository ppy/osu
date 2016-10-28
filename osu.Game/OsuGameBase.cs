using System;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
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
        internal OsuConfigManager Config = new OsuConfigManager();
        public BeatmapDatabase Beatmaps { get; private set; }

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public Options Options;
        public APIAccess API;

        protected override Container Content => ratioContainer;

        private RatioAdjust ratioContainer;

        public CursorContainer Cursor;

        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public OsuGameBase()
        {
            AddInternal(ratioContainer = new RatioAdjust());

            Children = new Drawable[]
            {
                Options = new Options(),
                Cursor = new OsuCursorContainer()
            };

            Beatmap.ValueChanged += Beatmap_ValueChanged;
        }

        private void Beatmap_ValueChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            OszArchiveReader.Register();
            Beatmaps = new BeatmapDatabase(Host.Storage, Host);

            //this completely overrides the framework default. will need to change once we make a proper FontStore.
            Fonts = new TextureStore() { ScaleAdjust = 0.01f };
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Regular"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/FontAwesome"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/osuFont"));

            API = new APIAccess()
            {
                Username = Config.Get<string>(OsuConfig.Username),
                Password = Config.Get<string>(OsuConfig.Password),
                Token = Config.Get<string>(OsuConfig.Token)
            };
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
