// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Represents a change which can be undone.
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
        public static void Apply(this IRevertibleChange change, HitObjectChangeHandler? changeHandler, bool commitImmediately = false)
        {
            if (changeHandler != null)
                changeHandler.Submit(change, commitImmediately);
            else
                change.Apply();
        }
    }
}
