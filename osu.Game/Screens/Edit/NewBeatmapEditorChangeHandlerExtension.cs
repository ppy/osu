// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Screens.Edit
{
    public static class NewBeatmapEditorChangeHandlerExtension
    {
        public static void SafeSubmit(this NewBeatmapEditorChangeHandler? manager, IRevertibleChange command, bool commitImmediately = false)
        {
            if (manager != null)
                manager.Submit(command, commitImmediately);
            else
                command.Apply();
        }
    }
}
