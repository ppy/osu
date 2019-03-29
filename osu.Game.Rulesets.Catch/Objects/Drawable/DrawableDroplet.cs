// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        private Pulp pulp;

        public override bool StaysOnPlate => false;

        public DrawableDroplet(Droplet h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2((float)CatchHitObject.OBJECT_RADIUS) / 4;
            Masking = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pulp = new Pulp { Size = Size });
        }

        public override Color4 AccentColour
        {
            get => base.AccentColour;
            set
            {
                base.AccentColour = value;
                pulp.AccentColour = AccentColour;
            }
        }
    }
}
