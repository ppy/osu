// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit.Tools
{
    public class SelectTool : HitObjectCompositionTool
    {
        public SelectTool()
            : base("Select")
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => null;
    }
}
