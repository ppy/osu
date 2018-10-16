// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public class SpinnerPlacementMask : PlacementMask
    {
        public SpinnerPlacementMask()
            : base(new Spinner())
        {
        }

        protected override void Update()
        {
            base.Update();

            HitObject.StartTime = EditorClock.CurrentTime;
        }
    }
}
