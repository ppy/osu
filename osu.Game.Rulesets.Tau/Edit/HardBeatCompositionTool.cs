using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Tau.Edit.Blueprints;
using osu.Game.Rulesets.Tau.Objects;

namespace osu.Game.Rulesets.Tau.Edit
{
    public class HardBeatCompositionTool : HitObjectCompositionTool
    {
        public HardBeatCompositionTool()
            : base(nameof(HardBeat))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new HardBeatPlacementBlueprint();
    }
}
