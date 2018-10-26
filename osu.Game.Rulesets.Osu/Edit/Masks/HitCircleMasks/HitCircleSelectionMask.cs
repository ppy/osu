// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Masks.HitCircleMasks.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Masks.HitCircleMasks
{
    public class HitCircleSelectionMask : SelectionMask
    {
        public HitCircleSelectionMask(DrawableHitCircle hitCircle)
            : base(hitCircle)
        {
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
            Position = hitCircle.Position;

            InternalChild = new HitCircleMask((HitCircle)hitCircle.HitObject);

            hitCircle.HitObject.PositionChanged += _ => Position = hitCircle.Position;
        }
    }
}
