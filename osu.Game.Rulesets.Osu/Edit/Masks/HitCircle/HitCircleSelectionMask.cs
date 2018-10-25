// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Masks.HitCircle.Components;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Masks.HitCircle
{
    public class HitCircleSelectionMask : SelectionMask
    {
        public HitCircleSelectionMask(DrawableHitCircle hitCircle)
            : base(hitCircle)
        {
            InternalChild = new HitCircleMask((Objects.HitCircle)hitCircle.HitObject);
        }
    }
}
