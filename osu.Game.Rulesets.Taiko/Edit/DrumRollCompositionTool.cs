using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Taiko.Edit.Blueprints;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class DrumRollCompositionTool : HitObjectCompositionTool
    {
        public DrumRollCompositionTool()
            : base(nameof(DrumRoll))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new DrumRollPlacementBlueprint();
    }
}
