// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Commands
{
    /// <summary>
    /// Represents a change which can be undone.
    /// </summary>
    public interface IEditorCommand
    {
        /// <summary>
        /// Applies this command to the current state.
        /// </summary>
        public void Apply();

        /// <summary>
        /// Creates a command which undoes the change of this command.
        /// </summary>
        /// <returns>The undo command.</returns>
        /// <remarks>Make sure to call this before calling <see cref="Apply"/>, as it reads the current state of the object.</remarks>
        public IEditorCommand CreateUndo();

        /// <summary>
        /// Whether this command would not have any meaningful effect if applied to the current state.
        /// </summary>
        public virtual bool IsRedundant => false;
    }
}
