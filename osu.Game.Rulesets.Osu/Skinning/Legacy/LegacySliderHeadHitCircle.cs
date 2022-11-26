// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacySliderHeadHitCircle : LegacyMainCirclePiece
    {
        [Resolved(canBeNull: true)]
        private DrawableHitObject? drawableHitObject { get; set; }

        private Drawable proxiedOverlayLayer = null!;

        public LegacySliderHeadHitCircle()
            : base("sliderstartcircle")
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            proxiedOverlayLayer = OverlayLayer.CreateProxy();

            if (drawableHitObject != null)
            {
                drawableHitObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(drawableHitObject);
            }
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            Debug.Assert(proxiedOverlayLayer.Parent == null);

            // see logic in LegacyReverseArrow.
            (drawableObject as DrawableSliderHead)?.DrawableSlider
                                                  .OverlayElementContainer.Add(proxiedOverlayLayer.With(d => d.Depth = float.MinValue));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableHitObject != null)
                drawableHitObject.HitObjectApplied -= onHitObjectApplied;
        }
    }
}
