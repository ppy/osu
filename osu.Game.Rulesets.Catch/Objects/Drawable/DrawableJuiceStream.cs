// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableJuiceStream : DrawableCatchHitObject<JuiceStream>
    {
        private readonly Container dropletContainer;

        public DrawableJuiceStream(JuiceStream s) : base(s)
        {
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.BottomLeft;
            X = 0;

            Child = dropletContainer = new Container { RelativeSizeAxes = Axes.Both, };

            foreach (CatchHitObject tick in s.NestedHitObjects.OfType<CatchHitObject>())
            {
                TinyDroplet tiny = tick as TinyDroplet;
                if (tiny != null)
                {
                    AddNested(new DrawableDroplet(tiny) { Scale = new Vector2(0.5f) });
                    continue;
                }

                Droplet droplet = tick as Droplet;
                if (droplet != null)
                    AddNested(new DrawableDroplet(droplet));

                Fruit fruit = tick as Fruit;
                if (fruit != null)
                    AddNested(new DrawableFruit(fruit));
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
