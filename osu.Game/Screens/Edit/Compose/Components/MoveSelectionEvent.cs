// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// An event which occurs when a <see cref="SelectionBlueprint{T}"/> is moved.
    /// </summary>
    public class MoveSelectionEvent<T>
    {
        /// <summary>
        /// The <see cref="SelectionBlueprint{T}"/> that triggered this <see cref="MoveSelectionEvent{T}"/>.
        /// </summary>
        public readonly SelectionBlueprint<T> Blueprint;

        /// <summary>
        /// The screen-space delta of this move event.
        /// </summary>
        public readonly Vector2 ScreenSpaceDelta;

        public MoveSelectionEvent(SelectionBlueprint<T> blueprint, Vector2 screenSpaceDelta)
        {
            Blueprint = blueprint;
            ScreenSpaceDelta = screenSpaceDelta;
        }
    }
}
