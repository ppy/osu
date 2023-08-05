// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class NoteSelectionBlueprint : ManiaSelectionBlueprint<Note>
    {
        public NoteSelectionBlueprint(Note note)
            : base(note)
        {
            AddInternal(new EditNotePiece { RelativeSizeAxes = Axes.X });
        }
    }
}
