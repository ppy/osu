// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        private Circle border;
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
            InternalChildren = new[]
            {
                pulp = new Pulp
                {
                    Size = Size,
                },
                border = new Circle
                {
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Radius = 4,
                        Colour = HitObject.HyperDash ? Color4.Red : AccentColour.Darken(1).Opacity(0.6f)
                    },
                    Size = new Vector2(Height * 4f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BorderColour = Color4.White,
                    BorderThickness = 4.0f,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Colour = AccentColour,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
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
