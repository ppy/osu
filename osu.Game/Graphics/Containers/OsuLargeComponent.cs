//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public class OsuLargeComponent : LargeContainer
    {
        public new OsuGameBase Game => base.Game as OsuGameBase;
    }
}
