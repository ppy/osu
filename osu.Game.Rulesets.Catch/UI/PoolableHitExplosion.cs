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

        public Color4 ObjectColour;

        public PalpableCatchHitObject HitObject { get; set; }

        public JudgementResult JudgementResult { get; set; }

        public float CatcherWidth { get; set; }

        public float CatchPosition { get; set; }

        private SkinnableDrawable skinnableExplosion { get; set; }

        public PoolableHitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = skinnableExplosion = new SkinnableDrawable(new CatchSkinComponent(CatchSkinComponents.HitExplosion), _ => new DefaultHitExplosion());
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            if (skinnableExplosion?.Drawable is CatchHitExplosion explosion)
            {
                explosion.JudgementResult = JudgementResult;
                explosion.ObjectColour = ObjectColour;
                explosion.HitObject = HitObject;
                explosion.CatchPosition = CatchPosition;
                explosion.CatcherWidth = CatcherWidth;

                explosion.RunAnimation();
            }

            this.Delay(DURATION).Then().Expire(true);
        }
    }
}
