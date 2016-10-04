using osu.Framework;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Processing;
using osu.Game.Online.API;
using osu.Game.Overlays;

namespace osu.Game
{
    public class OsuGameBase : BaseGame
    {
        internal OsuConfigManager Config = new OsuConfigManager();
        internal BeatmapDatabase Beatmaps { get; private set; }

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public Options Options;
        public APIAccess API;

        protected override Container Content => ratioContainer?.IsLoaded == true ? ratioContainer : base.Content;

        private RatioAdjust ratioContainer;

        public CursorContainer Cursor;

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Beatmaps = new BeatmapDatabase(Host.Storage);

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

            Add(new Drawable[]
            {
                ratioContainer = new RatioAdjust
                {
                    Children = new Drawable[]
                    {
                        Options = new Options(),
                        Cursor = new OsuCursorContainer()
                    }
                }
            });
        }

        protected override void Update()
        {
            base.Update();
            API.Update();
        }

        protected override void Dispose(bool isDisposing)
        {
            //refresh token may have changed.
            Config.Set(OsuConfig.Token, API.Token);
            Config.Save();

            base.Dispose(isDisposing);
        }
    }
}
