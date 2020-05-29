using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class TaikoBlueprintContainer : ComposeBlueprintContainer
    {
        public TaikoBlueprintContainer(IEnumerable<DrawableHitObject> hitObjects)
            : base(hitObjects)
        {
        }

        protected override SelectionHandler CreateSelectionHandler() => new TaikoSelectionHandler();

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) =>
            new TaikoSelectionBlueprint(hitObject);
    }
}
