//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundModeDefault : BackgroundMode
    {
        [BackgroundDependencyLoader]
        private void load(BaseGame game)
        {
            Add(new Background(@"Backgrounds/bg1"));
        }
    }
}