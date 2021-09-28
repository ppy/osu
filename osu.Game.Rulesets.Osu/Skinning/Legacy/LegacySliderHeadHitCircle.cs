// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySliderHeadHitCircle : LegacyMainCirclePiece
    {
        [Resolved(canBeNull: true)]
        private DrawableHitObject drawableHitObject { get; set; }

        private Drawable proxiedHitCircleOverlay;

        public LegacySliderHeadHitCircle()
            : base("sliderstartcircle")
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            proxiedHitCircleOverlay = HitCircleOverlay.CreateProxy();

            if (drawableHitObject != null)
            {
                drawableHitObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(drawableHitObject);
            }
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            Debug.Assert(proxiedHitCircleOverlay.Parent == null);

            // see logic in LegacyReverseArrow.
            (drawableObject as DrawableSliderHead)?.DrawableSlider
                                                  .OverlayElementContainer.Add(proxiedHitCircleOverlay.With(d => d.Depth = float.MinValue));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableHitObject != null)
                drawableHitObject.HitObjectApplied -= onHitObjectApplied;
        }
    }
}
