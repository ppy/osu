// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    internal class OptionLabel : OsuSpriteText
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Colour = colour.Gray6;
        }
    }
}
