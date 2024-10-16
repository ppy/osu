// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Represents a change which can be undone.
    /// </summary>
    public interface IRevertableChange
    {
        /// <summary>
        /// Applies this command to the current state.
        /// </summary>
        void Apply();

        /// <summary>
        /// Applies the inverse of this command to the current state.
        /// </summary>
        void Revert();
    }
}
