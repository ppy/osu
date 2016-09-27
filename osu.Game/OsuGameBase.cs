using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Configuration;
using osu.Game.Graphics.Processing;
using osu.Game.Online.API;

namespace osu.Game
{
    public class OsuGameBase : Framework.Game
    {
        internal OsuConfigManager Config = new OsuConfigManager();

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public APIAccess API;

        protected override Container AddTarget => ratioContainer?.IsLoaded == true ? ratioContainer : base.AddTarget;

        private RatioAdjust ratioContainer;

        public override void Load()
        {
            base.Load();

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
                        new CursorContainer()
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
