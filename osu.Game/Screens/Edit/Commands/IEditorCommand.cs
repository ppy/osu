// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Commands
{
    public interface IEditorCommand
    {
        public void Apply();

        public IEditorCommand CreateUndo();

        public bool IsRedundant => false;
    }
}
