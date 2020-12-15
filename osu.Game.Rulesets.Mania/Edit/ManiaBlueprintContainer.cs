// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaBlueprintContainer : ComposeBlueprintContainer
    {
        public ManiaBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableNote note:
                    return new NoteSelectionBlueprint(note);

                case DrawableHoldNote holdNote:
                    return new HoldNoteSelectionBlueprint(holdNote);
            }

            return base.CreateBlueprintFor(hitObject);
        }

        protected override SelectionHandler CreateSelectionHandler() => new ManiaSelectionHandler();
    }
}
