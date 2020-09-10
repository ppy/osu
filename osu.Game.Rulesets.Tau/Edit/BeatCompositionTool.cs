using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Tau.Edit.Blueprints;
using osu.Game.Rulesets.Tau.Objects;

namespace osu.Game.Rulesets.Tau.Edit
{
    public class BeatCompositionTool : HitObjectCompositionTool
    {
        public BeatCompositionTool()
            : base(nameof(Beat))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new BeatPlacementBlueprint();
    }
}
