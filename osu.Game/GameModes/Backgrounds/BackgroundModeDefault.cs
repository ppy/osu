//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Game.Graphics.Background;

namespace osu.Game.GameModes.Backgrounds
{
    public class BackgroundModeDefault : BackgroundMode
    {
        public override void Load(BaseGame game)
        {
            base.Load(game);

            Add(new Background());
        }
    }
}