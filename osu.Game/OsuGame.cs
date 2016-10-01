//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.OS;
using osu.Game.GameModes.Play;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public override void SetHost(BasicGameHost host)
        {
            base.SetHost(host);

            host.Size = new Vector2(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
        }

        public override void Load()
        {
            base.Load();

            Add(new MainMenu());
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
