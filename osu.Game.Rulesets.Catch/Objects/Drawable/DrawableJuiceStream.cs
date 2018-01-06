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
            Height = (float)HitObject.Duration;
            X = 0;

            Child = dropletContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                RelativeChildOffset = new Vector2(0, (float)HitObject.StartTime),
                RelativeChildSize = new Vector2(1, (float)HitObject.Duration)
            };

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

        protected override void AddNested(DrawableHitObject<CatchHitObject> h)
        {
            ((DrawableCatchHitObject)h).CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            dropletContainer.Add(h);
            base.AddNested(h);
        }
    }
}
