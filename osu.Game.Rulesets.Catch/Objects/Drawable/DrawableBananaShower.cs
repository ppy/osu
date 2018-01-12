// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableBananaShower : DrawableCatchHitObject<BananaShower>
    {
        private readonly Container bananaContainer;

        public DrawableBananaShower(BananaShower s)
            : base(s)
        {
            RelativeSizeAxes = Axes.X;
            Origin = Anchor.BottomLeft;
            X = 0;

            Child = bananaContainer = new Container { RelativeSizeAxes = Axes.Both };

            foreach (var b in s.NestedHitObjects.Cast<BananaShower.Banana>())
                AddNested(new DrawableFruit(b));
        }

        protected override void AddNested(DrawableHitObject h)
        {
            ((DrawableCatchHitObject)h).CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            bananaContainer.Add(h);
            base.AddNested(h);
        }
    }
}
