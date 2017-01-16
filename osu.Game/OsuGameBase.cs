//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Processing;
using osu.Game.Online.API;

namespace osu.Game
{
    public class OsuGameBase : BaseGame, IOnlineComponent
    {
        internal OsuConfigManager Config;

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public APIAccess API;

        protected override Container<Drawable> Content => ratioContainer;

        private RatioAdjust ratioContainer;

        public CursorContainer Cursor;

        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(this);
            Dependencies.Cache(Config);
            Dependencies.Cache(new BeatmapDatabase(Host.Storage, Host));
            Dependencies.Cache(new OsuColour());

            //this completely overrides the framework default. will need to change once we make a proper FontStore.
            Dependencies.Cache(Fonts = new FontStore { ScaleAdjust = 0.01f }, true);

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/FontAwesome"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/osuFont"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Medium"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-MediumItalic"));

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

            OszArchiveReader.Register();

            Dependencies.Cache(API = new APIAccess
            {
                Username = Config.Get<string>(OsuConfig.Username),
                Password = Config.Get<string>(OsuConfig.Password),
                Token = Config.Get<string>(OsuConfig.Token)
            });

            API.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    Config.Set(OsuConfig.Username, Config.Get<bool>(OsuConfig.SaveUsername) ? API.Username : string.Empty);
                    Config.Set(OsuConfig.Password, Config.Get<bool>(OsuConfig.SavePassword) ? API.Password : string.Empty);
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddInternal(ratioContainer = new RatioAdjust
            {
                Children = new[]
                {
                    Cursor = new OsuCursorContainer { Depth = float.MinValue }
                }
            });
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
