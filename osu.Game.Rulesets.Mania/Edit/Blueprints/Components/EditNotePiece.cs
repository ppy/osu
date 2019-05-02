// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public class EditNotePiece : CompositeDrawable
    {
        public EditNotePiece()
        {
            Height = NotePiece.NOTE_HEIGHT;

            CornerRadius = 5;
            Masking = true;

            InternalChild = new NotePiece();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
