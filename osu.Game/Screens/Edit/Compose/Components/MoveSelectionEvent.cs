// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// An event which occurs when a <see cref="OverlaySelectionBlueprint"/> is moved.
    /// </summary>
    public class MoveSelectionEvent
    {
        /// <summary>
        /// The <see cref="SelectionBlueprint"/> that triggered this <see cref="MoveSelectionEvent"/>.
        /// </summary>
        public readonly SelectionBlueprint Blueprint;

        /// <summary>
        /// The expected screen-space position of the hitobject at the current cursor position.
        /// </summary>
        public readonly Vector2 ScreenSpacePosition;

        /// <summary>
        /// The distance between <see cref="ScreenSpacePosition"/> and the hitobject's current position, in the coordinate-space of the hitobject's parent.
        /// </summary>
        public readonly Vector2 InstantDelta;

        public MoveSelectionEvent(SelectionBlueprint blueprint, Vector2 screenSpacePosition)
        {
            Blueprint = blueprint;
            ScreenSpacePosition = screenSpacePosition;

            InstantDelta = Blueprint.GetInstantDelta(ScreenSpacePosition);
        }
    }
}
