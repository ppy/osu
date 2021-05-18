// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class HoldNoteNoteOverlay : CompositeDrawable
    {
        private readonly HoldNoteSelectionBlueprint holdNoteBlueprint;
        private readonly HoldNotePosition position;

        public HoldNoteNoteOverlay(HoldNoteSelectionBlueprint holdNoteBlueprint, HoldNotePosition position)
        {
            this.holdNoteBlueprint = holdNoteBlueprint;
            this.position = position;

            InternalChild = new EditNotePiece { RelativeSizeAxes = Axes.X };
        }

        protected override void Update()
        {
            base.Update();

            var drawableObject = holdNoteBlueprint.DrawableObject;

            // Todo: This shouldn't exist, mania should not reference the drawable hitobject directly.
            if (drawableObject.IsLoaded)
            {
                DrawableNote note = position == HoldNotePosition.Start ? (DrawableNote)drawableObject.Head : drawableObject.Tail;

                Anchor = note.Anchor;
                Origin = note.Origin;

                Size = note.DrawSize;
                Position = note.DrawPosition;
            }
        }
    }
}
