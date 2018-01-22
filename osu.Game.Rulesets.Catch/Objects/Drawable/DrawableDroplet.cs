// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        public DrawableDroplet(Droplet h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2((float)CatchHitObject.OBJECT_RADIUS) / 4;
            AccentColour = h.ComboColour;
            Masking = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Pulp
            {
                AccentColour = AccentColour,
                Size = Size
            };
        }
    }
}
