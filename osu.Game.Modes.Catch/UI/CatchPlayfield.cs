//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.UI;
using OpenTK;

namespace osu.Game.Modes.Catch.UI
{
    public class CatchPlayfield : Playfield
    {
        public CatchPlayfield()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(512, 0.9f);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Add(new Box { RelativeSizeAxes = Axes.Both, Alpha = 0.5f });
        }
    }
}