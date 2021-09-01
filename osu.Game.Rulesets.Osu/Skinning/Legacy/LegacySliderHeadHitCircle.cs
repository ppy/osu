// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public LegacySliderHeadHitCircle()
            : base("sliderstartcircle")
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // see logic in LegacyReverseArrow.
            (drawableHitObject as DrawableSliderHead)?.DrawableSlider
                                                     .OverlayElementContainer.Add(HitCircleOverlay.CreateProxy().With(d => d.Depth = float.MinValue));
        }
    }
}
