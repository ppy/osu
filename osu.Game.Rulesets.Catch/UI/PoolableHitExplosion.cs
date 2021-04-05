// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        [Cached]
        public readonly Bindable<Color4> ObjectColour = new Bindable<Color4>();

        [Cached]
        public readonly Bindable<PalpableCatchHitObject> HitObject = new Bindable<PalpableCatchHitObject>();

        [Cached]
        public readonly Bindable<JudgementResult> JudgementResult = new Bindable<JudgementResult>();

        [Cached(Name = "CatcherWidth")]
        public readonly Bindable<float> CatcherWidth = new Bindable<float>();

        [Cached]
        public readonly Bindable<float> CatchPosition = new Bindable<float>();

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
                explosion.RunAnimation();
            }

            this.Delay(DURATION).Then().Expire(true);
        }
    }
}
