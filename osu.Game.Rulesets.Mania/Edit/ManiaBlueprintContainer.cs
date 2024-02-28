// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Mania.Edit
{
    public partial class ManiaBlueprintContainer : ComposeBlueprintContainer
    {
        public ManiaBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Note note:
                    return new NoteSelectionBlueprint(note);

                case HoldNote holdNote:
                    return new HoldNoteSelectionBlueprint(holdNote);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new ManiaSelectionHandler();

        protected sealed override DragBox CreateDragBox() => new ScrollingDragBox(Composer.Playfield);
    }
}
