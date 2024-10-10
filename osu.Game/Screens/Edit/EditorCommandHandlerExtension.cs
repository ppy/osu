// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Screens.Edit
{
    public static class EditorCommandHandlerExtension
    {
        public static void SafeSubmit(this EditorCommandHandler? manager, IEditorCommand command, bool commitImmediately = false)
        {
            if (manager != null)
                manager.Submit(command, commitImmediately);
            else
                command.Apply();
        }

        public static void SafeSubmit(this EditorCommandHandler? manager, IEnumerable<IEditorCommand> commands, bool commitImmediately = false)
        {
            if (manager != null)
                manager.Submit(commands, commitImmediately);
            else
            {
                foreach (var command in commands)
                    command.Apply();
            }
        }
    }
}
