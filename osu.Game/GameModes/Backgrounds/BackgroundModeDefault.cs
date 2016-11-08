//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Game.Graphics.Background;

namespace osu.Game.GameModes.Backgrounds
{
    public class BackgroundModeDefault : BackgroundMode
    {
        [Initializer]
        private void Load(BaseGame game)
        {
            Add(new Background());
        }
    }
}