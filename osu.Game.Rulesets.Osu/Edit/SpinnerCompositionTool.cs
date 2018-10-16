// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Edit.Masks;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class SpinnerCompositionTool : HitObjectCompositionTool
    {
        public SpinnerCompositionTool()
            : base("Spinner")
        {
        }

        public override PlacementMask CreatePlacementMask() => new SpinnerPlacementMask();
    }
}
