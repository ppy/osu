// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableBananaShower : DrawableCatchHitObject<BananaShower>
    {
        private readonly Container bananaContainer;

        public DrawableBananaShower(BananaShower s, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> createDrawableRepresentation = null)
            : base(s)
        {
            RelativeSizeAxes = Axes.X;
            Origin = Anchor.BottomLeft;
            X = 0;

            AddInternal(bananaContainer = new Container { RelativeSizeAxes = Axes.Both });

            foreach (var b in s.NestedHitObjects.Cast<Banana>())
                AddNested(createDrawableRepresentation?.Invoke(b));
        }

        protected override void AddNested(DrawableHitObject h)
        {
            ((DrawableCatchHitObject)h).CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            bananaContainer.Add(h);
            base.AddNested(h);
        }
    }
}
