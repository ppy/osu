// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Edit.Blueprints;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class GridFromPointsTool : CompositionTool
    {
        public GridFromPointsTool()
            : base("Change grid")
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new GridPlacementBlueprint();
    }
}
