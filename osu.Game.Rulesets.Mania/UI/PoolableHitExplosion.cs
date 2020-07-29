// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI
{
    public class PoolableHitExplosion : PoolableDrawable
    {
        public const double DURATION = 200;

        [Resolved]
        private Column column { get; set; }

        private SkinnableDrawable skinnableExplosion;

        public PoolableHitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = skinnableExplosion = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitExplosion, column.Index),
                _ => new DefaultHitExplosion(column.AccentColour, false /*todo */))
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            (skinnableExplosion?.Drawable as IHitExplosion)?.Animate();

            this.Delay(DURATION).Then().Expire();
        }
    }
}
