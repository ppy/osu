// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Screens.Edit
{
    public static class EditorCommandManagerExtension
    {
        public static void SafeSubmit(this EditorCommandHandler? manager, IEditorCommand command, bool commitImmediately = false)
        {
            if (manager != null)
                manager.Submit(command, commitImmediately);
            else
                command.Apply();
        }
    }
}