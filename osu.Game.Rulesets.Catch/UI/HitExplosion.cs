// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class HitExplosion : PoolableDrawableWithLifetime<HitExplosionEntry>
    {
        private readonly SkinnableDrawable skinnableExplosion;

        public HitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            InternalChild = skinnableExplosion = new SkinnableDrawable(new CatchSkinComponentLookup(CatchSkinComponents.HitExplosion), _ => new DefaultHitExplosion())
            {
                CentreComponent = false,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre
            };
        }

        protected override void OnApply(HitExplosionEntry entry)
        {
            base.OnApply(entry);
            if (IsLoaded)
                apply(entry);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            apply(Entry);
        }

        private void apply(HitExplosionEntry? entry)
        {
            if (entry == null)
                return;

            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);

            (skinnableExplosion.Drawable as IHitExplosion)?.Animate(entry);
            LifetimeEnd = skinnableExplosion.Drawable.LatestTransformEndTime;
        }
    }
}
