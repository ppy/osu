//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Drawing;
using osu.Framework.Framework;
using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Processing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game
{
    public class OsuGame : Framework.Game
    {
        internal OsuConfigManager Config = new OsuConfigManager();

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        internal APIAccess API;

        public override void Load()
        {
            base.Load();

            Window.Size = new Size(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
            Window.OnSizeChanged += window_OnSizeChanged;

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

            AddProcessingContainer(new RatioAdjust());

            //Add(new FontTest());

            Add(new MainMenu());
            Add(new CursorContainer());
        }

        protected override void Dispose(bool isDisposing)
        {
            //refresh token may have changed.
            Config.Set(OsuConfig.Token, API.Token);

            base.Dispose(isDisposing);
        }

        private void window_OnSizeChanged()
        {
            //don't store window size if window is minimized
            if(!Window.IsMinimized)
            {
                Config.Set<int>(OsuConfig.Width, Window.Size.Width);
                Config.Set<int>(OsuConfig.Height, Window.Size.Height);
            }
        }
    }
}
