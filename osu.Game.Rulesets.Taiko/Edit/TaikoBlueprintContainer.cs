// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Edit.Blueprints;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class TaikoBlueprintContainer : ComposeBlueprintContainer
    {
        public TaikoBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        protected override SelectionHandler CreateSelectionHandler() => new TaikoSelectionHandler();

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) =>
            new TaikoSelectionBlueprint(hitObject);
    }
}
