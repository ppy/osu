// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Represents a single atomic change which can be undone and saved to undo history.
    /// </summary>
    public interface IRevertibleChange
    {
        /// <summary>
        /// Applies this change to the current state.
        /// </summary>
        void Apply();

        /// <summary>
        /// Applies the inverse of this change to the current state.
        /// </summary>
        void Revert();
    }

    public static class RevertibleChangeExtension
    {
        public static void Apply(this IRevertibleChange change, IEditorChangeHandler? changeHandler)
        {
            changeHandler?.BeginChange();
            change.Apply();
            changeHandler?.Record(change);
            changeHandler?.EndChange();
        }
    }
}
