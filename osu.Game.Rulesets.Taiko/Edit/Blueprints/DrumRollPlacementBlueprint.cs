using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public class DrumRollPlacementBlueprint : TaikoSpanPlacementBlueprint
    {
        public DrumRollPlacementBlueprint()
            : base(new DrumRoll())
        {
        }
    }
}
