using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Taiko.Edit.Blueprints;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class HitCompositionTool : HitObjectCompositionTool
    {
        public HitCompositionTool()
            : base(nameof(Hit))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new HitPlacementBlueprint();
    }
}
