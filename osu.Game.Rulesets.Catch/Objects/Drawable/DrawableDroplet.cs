// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        private Border border;
        private Pulp pulp;

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
            InternalChildren = new Circle[]
            {
                pulp = new Pulp
                {
                    Size = Size,
                },
                border = new Border(4.0f, new Vector2(Height * 4.0f), 4.0f, AccentColour, false),
            };
        }

        protected override void Update()
        {
            base.Update();

            border.Alpha = (float)MathHelper.Clamp((HitObject.StartTime - Time.Current) / 50, 0, 1);
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;
                pulp.AccentColour = AccentColour;
            }
        }
    }
}
