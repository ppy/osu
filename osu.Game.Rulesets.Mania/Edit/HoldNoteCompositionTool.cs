// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
