// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class HoldNoteNoteSelectionBlueprint : NoteSelectionBlueprint
    {
        public HoldNoteNoteSelectionBlueprint(DrawableNote note)
            : base(note)
        {
            Select();
        }

        protected override void Update()
        {
            base.Update();

            Anchor = DrawableObject.Anchor;
            Origin = DrawableObject.Origin;

            Position = DrawableObject.DrawPosition;
        }

        // Todo: This is temporary, since the note masks don't do anything special yet. In the future they will handle input.
        public override bool HandlePositionalInput => false;
    }
}
