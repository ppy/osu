// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Sprites
{
    class OsuSpriteText : SpriteText
    {
        public const float FONT_SIZE = 16;

        public OsuSpriteText()
        {
            Shadow = true;
            TextSize = FONT_SIZE;
        }
    }
}
