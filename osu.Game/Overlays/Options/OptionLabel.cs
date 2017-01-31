//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    class OptionLabel : SpriteText
    {
        public OptionLabel()
        {
            TextSize = OptionsOverlay.FONT_SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Colour = colour.Gray6;
        }
    }
}
