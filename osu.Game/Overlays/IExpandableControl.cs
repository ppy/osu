// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays
{
    /// <summary>
    /// An interface for UI controls with the ability to expand/contract.
    /// </summary>
    public interface IExpandableControl : IExpandable
    {
        /// <summary>
        /// Returns whether the UI control is currently in a dragged state.
        /// </summary>
        bool IsControlDragged { get; }
    }
}
