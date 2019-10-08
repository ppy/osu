// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// An event which occurs when a <see cref="SelectionBlueprint"/> is dragged.
    /// </summary>
    public class SelectionDragEvent
    {
        /// <summary>
        /// The dragged <see cref="SelectionBlueprint"/>.
        /// </summary>
        public readonly SelectionBlueprint DraggedBlueprint;

        /// <summary>
        /// The screen-space position of the hitobject at the start of the drag.
        /// </summary>
        public readonly Vector2 ScreenSpaceDragStartPosition;

        /// <summary>
        /// The new screen-space position of the hitobject at the current drag point.
        /// </summary>
        public readonly Vector2 ScreenSpaceDragPosition;

        /// <summary>
        /// The distance between <see cref="ScreenSpaceDragPosition"/> and the hitobject's current position, in the coordinate-space of the hitobject's parent.
        /// </summary>
        /// <remarks>
        /// This does not use <see cref="ScreenSpaceDragStartPosition"/> and does not represent the cumulative drag distance.
        /// </remarks>
        public readonly Vector2 InstantDragDelta;

        public SelectionDragEvent(SelectionBlueprint blueprint, Vector2 screenSpaceDragStartPosition, Vector2 screenSpaceDragPosition)
        {
            DraggedBlueprint = blueprint;
            ScreenSpaceDragStartPosition = screenSpaceDragStartPosition;
            ScreenSpaceDragPosition = screenSpaceDragPosition;

            InstantDragDelta = toLocalSpace(ScreenSpaceDragPosition) - DraggedBlueprint.HitObject.Position;
        }

        /// <summary>
        /// Converts a screen-space position into the coordinate space of the hitobject's parents.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The position in the coordinate space of the hitobject's parent.</returns>
        private Vector2 toLocalSpace(Vector2 screenSpacePosition) => DraggedBlueprint.HitObject.Parent.ToLocalSpace(screenSpacePosition);
    }
}
