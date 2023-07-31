// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public partial class EditNotePiece : CompositeDrawable
    {
        public EditNotePiece()
        {
            Height = DefaultNotePiece.NOTE_HEIGHT;

            CornerRadius = 5;
            Masking = true;

            InternalChild = new DefaultNotePiece();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
