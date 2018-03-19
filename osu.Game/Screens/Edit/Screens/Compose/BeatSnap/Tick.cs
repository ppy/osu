// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.BeatSnap
{
    public class Tick : Box
    {
        private readonly int divisor;

        public Tick(int divisor)
        {
            this.divisor = divisor;

            Size = new Vector2(2, 10);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (divisor >= 16)
                Colour = colours.Red;
            else if (divisor >= 8)
                Colour = colours.Yellow;
            else
                Colour = colours.Gray4;
        }
    }
}
