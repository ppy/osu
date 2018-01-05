// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : DrawableCatchHitObject<Droplet>
    {
        public DrawableDroplet(Droplet h)
            : base(h)
        {
            Origin = Anchor.Centre;

            Size = new Vector2(Pulp.PULP_SIZE);

            AccentColour = h.ComboColour;
            Masking = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Pulp
            {
                AccentColour = AccentColour,
                Scale = new Vector2(0.8f),
            };
        }
    }
}
