// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// Provides a border around the playfield.
    /// </summary>
    public class EditorPlayfieldBorder : CompositeDrawable
    {
        public EditorPlayfieldBorder()
        {
            RelativeSizeAxes = Axes.Both;

            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 2;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                AlwaysPresent = true
            };
        }
    }
}
