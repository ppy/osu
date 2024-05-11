// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public partial class DrawableBananaShower : DrawableCatchHitObject
    {
        private readonly Container bananaContainer;

        public DrawableBananaShower()
            : this(null)
        {
        }

        public DrawableBananaShower(BananaShower? s)
            : base(s)
        {
            RelativeSizeAxes = Axes.X;
            Origin = Anchor.BottomLeft;

            AddInternal(bananaContainer = new NestedFruitContainer { RelativeSizeAxes = Axes.Both });
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            bananaContainer.Add(hitObject);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            bananaContainer.Clear(false);
        }
    }
}
