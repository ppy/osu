// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Dialog
{
    /// <summary>
    /// A dialog which provides confirmation for deletion of something.
    /// </summary>
    public abstract partial class DeletionDialog : DangerousActionDialog
    {
        protected DeletionDialog()
        {
            HeaderText = DialogStrings.DeletionHeaderText;
            Icon = FontAwesome.Solid.Trash;
        }
    }
}
