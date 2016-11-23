//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    class DifficultyIcon : Container
    {
        public DifficultyIcon(FontAwesome icon, Color4 color)
        {
            const float size = 20;
            Size = new Vector2(size);
            Children = new[]
            {
                new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    TextSize = size,
                    Colour = color,
                    Icon = icon
                }
            };
        }
    }
}