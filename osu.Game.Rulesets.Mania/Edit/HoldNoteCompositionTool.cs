// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Edit.Blueprints;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class HoldNoteCompositionTool : HitObjectCompositionTool
    {
        public HoldNoteCompositionTool()
            : base("Hold")
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new HoldNotePlacementBlueprint();
    }
}
