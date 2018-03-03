// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection
{
    public class OsuHitObjectOverlayLayer : HitObjectOverlayLayer
    {
        protected override HitObjectOverlay CreateOverlayFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableHitCircle circle:
                    return new HitCircleOverlay(circle);
                case DrawableSlider slider:
                    return new SliderOverlay(slider);
            }

            return base.CreateOverlayFor(hitObject);
        }
    }
}
