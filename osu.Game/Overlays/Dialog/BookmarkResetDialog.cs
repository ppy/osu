// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Edit;

namespace osu.Game.Overlays.Dialog
{
    public partial class BookmarkResetDialog : DangerousActionDialog
    {
        private readonly EditorBeatmap editor;

        public BookmarkResetDialog(EditorBeatmap editorBeatmap)
        {
            HeaderText = "Are you sure you want to reset all bookmarks?\nThis action is also undoable.";
            Icon = FontAwesome.Solid.ExclamationTriangle;
            editor = editorBeatmap;
            DangerousAction = () => editor.Bookmarks.Clear();
        }
    }
}
