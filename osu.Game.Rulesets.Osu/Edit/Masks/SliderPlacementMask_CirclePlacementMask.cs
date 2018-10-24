// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public partial class SliderPlacementMask
    {
        private class CirclePlacementMask : PlacementMask
        {
            public CirclePlacementMask(HitCircle hitCircle)
                : base(hitCircle)
            {
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                InternalChild = new HitCircleMask(hitCircle);

                hitCircle.PositionChanged += _ => Position = hitCircle.StackedPosition;
            }

            protected override bool Handle(UIEvent e) => false;
        }
    }
}
