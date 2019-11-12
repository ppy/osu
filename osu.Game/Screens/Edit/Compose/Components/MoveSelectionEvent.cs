// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// An event which occurs when a <see cref="SelectionBlueprint"/> is moved.
    /// </summary>
    public class MoveSelectionEvent
    {
        /// <summary>
        /// The <see cref="SelectionBlueprint"/> that triggered this <see cref="MoveSelectionEvent"/>.
        /// </summary>
        public readonly SelectionBlueprint Blueprint;

        /// <summary>
        /// The starting screen-space position of the hitobject.
        /// </summary>
        public readonly Vector2 ScreenSpaceStartPosition;

        /// <summary>
        /// The expected screen-space position of the hitobject at the current cursor position.
        /// </summary>
        public readonly Vector2 ScreenSpacePosition;

        /// <summary>
        /// The distance between <see cref="ScreenSpacePosition"/> and the hitobject's current position, in the coordinate-space of the hitobject's parent.
        /// </summary>
        /// <remarks>
        /// This does not use <see cref="ScreenSpaceStartPosition"/> and does not represent the cumulative movement distance.
        /// </remarks>
        public readonly Vector2 InstantDelta;

        public MoveSelectionEvent(SelectionBlueprint blueprint, Vector2 screenSpaceStartPosition, Vector2 screenSpacePosition)
        {
            Blueprint = blueprint;
            ScreenSpaceStartPosition = screenSpaceStartPosition;
            ScreenSpacePosition = screenSpacePosition;

            InstantDelta = Blueprint.DrawableObject.Parent.ToLocalSpace(ScreenSpacePosition) - Blueprint.DrawableObject.Position;
        }
    }
}
