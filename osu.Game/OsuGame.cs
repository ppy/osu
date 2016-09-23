﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using osu.Game.Graphics.Processing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.OS;

namespace osu.Game
{
    public class OsuGame : Framework.Game
    {
        internal OsuConfigManager Config = new OsuConfigManager();

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        internal APIAccess API;

        public override void SetHost(BasicGameHost host)
        {
            base.SetHost(host);

            host.Size = new Vector2(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
        }

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

			//var req = new ListChannelsRequest();
			//req.Success += content =>
			//{
			//};
			//API.Queue(req);

			Children = new Drawable[]
			{
				new RatioAdjust
				{
					Children = new Drawable[]
					{
						new MainMenu(),
						new CursorContainer()
					}
				}
			};
        }

        protected override void Dispose(bool isDisposing)
        {
            //refresh token may have changed.
            Config.Set(OsuConfig.Token, API.Token);

            base.Dispose(isDisposing);
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if (!base.Invalidate(invalidation, source, shallPropagate)) return false;
            
            if (Parent != null)
            {
                Config.Set(OsuConfig.Width, ActualSize.X);
                Config.Set(OsuConfig.Height, ActualSize.Y);
            }
            return true;
        }
    }
}
