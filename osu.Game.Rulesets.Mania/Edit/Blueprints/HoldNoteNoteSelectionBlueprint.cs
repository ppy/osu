// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class HoldNoteNoteSelectionBlueprint : ManiaSelectionBlueprint<HoldNote>
    {
        protected new DrawableHoldNote DrawableObject => (DrawableHoldNote)base.DrawableObject;

        private readonly HoldNotePosition position;

        public HoldNoteNoteSelectionBlueprint(HoldNote holdNote, HoldNotePosition position)
            : base(holdNote)
        {
            this.position = position;
            InternalChild = new EditNotePiece { RelativeSizeAxes = Axes.X };

            Select();
        }

        protected override void Update()
        {
            base.Update();

            // Todo: This shouldn't exist, mania should not reference the drawable hitobject directly.
            if (DrawableObject.IsLoaded)
            {
                DrawableNote note = position == HoldNotePosition.Start ? (DrawableNote)DrawableObject.Head : DrawableObject.Tail;

                Anchor = note.Anchor;
                Origin = note.Origin;

                Size = note.DrawSize;
                Position = note.DrawPosition;
            }
        }

        // Todo: This is temporary, since the note masks don't do anything special yet. In the future they will handle input.
        public override bool HandlePositionalInput => false;
    }
}
