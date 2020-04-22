// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Interface for a component that manages changes in the <see cref="Editor"/>.
    /// </summary>
    public interface IEditorChangeHandler
    {
        /// <summary>
        /// Begins a bulk state change event. <see cref="EndChange"/> should be invoked soon after.
        /// </summary>
        /// <remarks>
        /// This should be invoked when multiple changes to the <see cref="Editor"/> should be bundled together into one state change event.
        /// When nested invocations are involved, a state change will not occur until an equal number of invocations of <see cref="EndChange"/> are received.
        /// </remarks>
        /// <example>
        /// When a group of <see cref="HitObject"/>s are deleted, a single undo and redo state change should update the state of all <see cref="HitObject"/>.
        /// </example>
        void BeginChange();

        /// <summary>
        /// Ends a bulk state change event.
        /// </summary>
        /// <remarks>
        /// This should be invoked as soon as possible after <see cref="BeginChange"/> to cause a state change.
        /// </remarks>
        void EndChange();
    }
}
