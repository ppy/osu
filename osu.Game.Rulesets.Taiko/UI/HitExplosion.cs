// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A circle explodes from the hit target to indicate a hitobject has been hit.
    /// </summary>
    internal class HitExplosion : CircularContainer
    {
        public override bool RemoveWhenNotAlive => true;

        [Cached(typeof(DrawableHitObject))]
        public readonly DrawableHitObject JudgedObject;

        private readonly HitResult result;

        private SkinnableDrawable skinnable;

        public override double LifetimeStart => skinnable.Drawable.LifetimeStart;

        public override double LifetimeEnd => skinnable.Drawable.LifetimeEnd;

        public HitExplosion(DrawableHitObject judgedObject, HitResult result)
        {
            JudgedObject = judgedObject;
            this.result = result;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(TaikoHitObject.DEFAULT_SIZE);

            RelativePositionAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = skinnable = new SkinnableDrawable(new TaikoSkinComponent(getComponentName(result)), _ => new DefaultHitExplosion(JudgedObject, result));
        }

        private static TaikoSkinComponents getComponentName(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return TaikoSkinComponents.TaikoExplosionMiss;

                case HitResult.Ok:
                    return TaikoSkinComponents.TaikoExplosionOk;

                case HitResult.Great:
                    return TaikoSkinComponents.TaikoExplosionGreat;
            }

            throw new ArgumentOutOfRangeException(nameof(result), $"Invalid result type: {result}");
        }

        /// <summary>
        /// Transforms this hit explosion to visualise a secondary hit.
        /// </summary>
        public void VisualiseSecondHit()
        {
            this.ResizeTo(new Vector2(TaikoStrongableHitObject.DEFAULT_STRONG_SIZE), 50);
        }
    }
}
