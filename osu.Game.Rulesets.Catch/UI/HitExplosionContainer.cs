// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class HitExplosionContainer : PooledDrawableWithLifetimeContainer<HitExplosionEntry, HitExplosion>
    {
        protected override bool RemoveRewoundEntry => true;

        private readonly DrawablePool<HitExplosion> pool;

        public HitExplosionContainer()
        {
            RelativeSizeAxes = Axes.Both;

            AddInternal(pool = new DrawablePool<HitExplosion>(10));
        }

        protected override HitExplosion GetDrawable(HitExplosionEntry entry) => pool.Get(d => d.Apply(entry));
    }
}
