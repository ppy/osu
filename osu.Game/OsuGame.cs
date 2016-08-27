//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Drawing;
using osu.Framework.Framework;
using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Processing;

namespace osu.Game
{
    public class OsuGame : Framework.Game
    {
        internal OsuConfigManager Config = new OsuConfigManager();

        protected override string MainResourceFile => @"osu.Game.Resources.dll";

        public override void Load()
        {
            base.Load();

            Window.Size = new Size(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
            Window.OnSizeChanged += window_OnSizeChanged;

            Window.Title = "osu!";

            AddProcessingContainer(new RatioAdjust());

            //Add(new FontTest());

            Add(new MainMenu());
            Add(new CursorContainer());
        }

        private void window_OnSizeChanged()
        {
            Config.Set<int>(OsuConfig.Width, Window.Size.Width);
            Config.Set<int>(OsuConfig.Height, Window.Size.Height);
        }
    }
}
