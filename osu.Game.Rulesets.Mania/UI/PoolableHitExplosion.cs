// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class PoolableHitExplosion : PoolableDrawable
    {
        public const double DURATION = 200;

        public Judgement Result { get; private set; }

        private SkinnableDrawable skinnableExplosion;

        public PoolableHitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = skinnableExplosion = new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.HitExplosion), _ => new DefaultHitExplosion())
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        public void Apply(Judgement result)
        {
            Result = result;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            LifetimeStart = Time.Current;

            (skinnableExplosion?.Drawable as IHitExplosion)?.Animate(Result);

            this.Delay(DURATION).Then().Expire();
        }
    }
}
