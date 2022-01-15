// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI
{
    public class PoolableHitExplosion : PoolableDrawable
    {
        public const double DURATION = 200;

        public JudgementResult Result { get; private set; }

        private SkinnableDrawable skinnableExplosion;

        public PoolableHitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = skinnableExplosion = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitExplosion), _ => new DefaultHitExplosion())
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        public void Apply(JudgementResult result)
        {
            Result = result;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            (skinnableExplosion?.Drawable as IHitExplosion)?.Animate(Result);

            this.Delay(DURATION).Then().Expire();
        }
    }
}
