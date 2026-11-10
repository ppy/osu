// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class BookmarkResetDialog : DeletionDialog
    {
        private readonly EditorBeatmap editor;

        public BookmarkResetDialog(EditorBeatmap editorBeatmap)
        {
            editor = editorBeatmap;
            BodyText = EditorDialogsStrings.AllBookmarks;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            DangerousAction = () => editor.Bookmarks.Clear();
        }
    }
}
