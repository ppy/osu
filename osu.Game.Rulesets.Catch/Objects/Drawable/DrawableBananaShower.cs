// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableBananaShower : DrawableCatchHitObject<BananaShower>
    {
        private readonly Container dropletContainer;

        public DrawableBananaShower(BananaShower s) : base(s)
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

            foreach (var b in s.NestedHitObjects.OfType<BananaShower.Banana>())
                    AddNested(new DrawableFruit(b));
        }

        protected override void AddNested(DrawableHitObject<CatchHitObject> h)
        {
            ((DrawableCatchHitObject)h).CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            dropletContainer.Add(h);
            base.AddNested(h);
        }
    }
}
