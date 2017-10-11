// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
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

            var start = new DrawableFruit(new Fruit
            {
                Samples = s.Samples,
                ComboColour = Color4.Blue,
                StartTime = s.StartTime,
                X = s.X,
            });

            AddNested(start);

            var end = new DrawableFruit(new Fruit
            {
                Samples = s.Samples,
                ComboColour = Color4.Red,
                StartTime = s.EndTime,
                X = s.EndX,
            });

            AddNested(end);

            foreach (var tick in s.Ticks)
            {
                var droplet = new DrawableDroplet(tick);
                AddNested(droplet);
            }
        }

        protected override void AddNested(DrawableHitObject<CatchBaseHit> h)
        {
            ((DrawableCatchHitObject)h).CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            dropletContainer.Add(h);
            base.AddNested(h);
        }
    }
}
