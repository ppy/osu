// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Taiko.Edit.Blueprints;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class HitCompositionTool : HitObjectCompositionTool
    {
        public HitCompositionTool()
            : base("音符")
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new HitPlacementBlueprint();
    }
}
