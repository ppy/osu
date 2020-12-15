// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class NoteSelectionBlueprint : ManiaSelectionBlueprint
    {
        public NoteSelectionBlueprint(DrawableNote note)
            : base(note)
        {
            AddInternal(new EditNotePiece { RelativeSizeAxes = Axes.X });
        }

        protected override void Update()
        {
            base.Update();

            // Todo: This shouldn't exist, mania should not reference the drawable hitobject directly.
            if (DrawableObject.IsLoaded)
                Size = DrawableObject.DrawSize;
        }
    }
}
