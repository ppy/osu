// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableJuiceStream : DrawableCatchHitObject<JuiceStream>
    {
        private readonly Container dropletContainer;

        public DrawableJuiceStream(JuiceStream s)
            : base(s)
        {
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.BottomLeft;
            X = 0;

            Child = dropletContainer = new Container { RelativeSizeAxes = Axes.Both, };

            foreach (var tick in s.NestedHitObjects)
            {
                switch (tick)
                {
                    case TinyDroplet tiny:
                        AddNested(new DrawableDroplet(tiny) { Scale = new Vector2(0.5f) });
                        break;
                    case Droplet droplet:
                        AddNested(new DrawableDroplet(droplet));
                        break;
                    case Fruit fruit:
                        AddNested(new DrawableFruit(fruit));
                        break;
                }
            }
        }

        protected override void AddNested(DrawableHitObject h)
        {
            var catchObject = (DrawableCatchHitObject)h;

            catchObject.CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            catchObject.AccentColour = HitObject.ComboColour;

            dropletContainer.Add(h);
            base.AddNested(h);
        }
    }
}
