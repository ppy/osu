// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class PoolableHitExplosion : PoolableDrawable
    {
        public const double DURATION = 1000;

        private Color4 objectColour;

        public Color4 ObjectColour
        {
            get => objectColour;
            set
            {
                if (objectColour == value)
                    return;

                objectColour = value;

                if (skinnableExplosion.Drawable is ICatchHitExplosion hitExplosion)
                {
                    hitExplosion.ObjectColour = value;
                }
            }
        }

        public PalpableCatchHitObject HitObject;

        public JudgementResult JudgementResult;

        public float CatcherMargin;

        public float CatcherWidth;

        public float CatchPosition;

        private SkinnableDrawable skinnableExplosion;

        public PoolableHitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = skinnableExplosion = new SkinnableDrawable(new CatchSkinComponent(CatchSkinComponents.LightingGlow), _ => new DefaultHitExplosion());
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            if (skinnableExplosion?.Drawable is ICatchHitExplosion explosion)
            {
                explosion.JudgementResult = JudgementResult;
                explosion.ObjectColour = ObjectColour;
                explosion.HitObject = HitObject;
                explosion.CatchPosition = CatchPosition;
                explosion.CatcherMargin = CatcherMargin;
                explosion.CatcherWidth = CatcherWidth;

                explosion.Animate();
            }

            this.Delay(DURATION).Then().Expire(true);
        }
    }
}
