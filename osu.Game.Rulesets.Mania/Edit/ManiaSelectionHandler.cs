// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaSelectionHandler : SelectionHandler
    {
        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private IManiaHitObjectComposer composer { get; set; }

        private IClock editorClock;

        [BackgroundDependencyLoader]
        private void load(IAdjustableClock clock)
        {
            editorClock = clock;
        }

        public override void HandleMovement(MoveSelectionEvent moveEvent)
        {
            var maniaBlueprint = (ManiaSelectionBlueprint)moveEvent.Blueprint;
            int lastColumn = maniaBlueprint.DrawableObject.HitObject.Column;

            adjustOrigins(maniaBlueprint);
            performDragMovement(moveEvent);
            performColumnMovement(lastColumn, moveEvent);

            base.HandleMovement(moveEvent);
        }

        /// <summary>
        /// Ensures that the position of hitobjects remains centred to the mouse position.
        /// E.g. The hitobject position will change if the editor scrolls while a hitobject is dragged.
        /// </summary>
        /// <param name="reference">The <see cref="ManiaSelectionBlueprint"/> that received the drag event.</param>
        private void adjustOrigins(ManiaSelectionBlueprint reference)
        {
            var referenceParent = (HitObjectContainer)reference.DrawableObject.Parent;

            float offsetFromReferenceOrigin = reference.DragPosition.Y - reference.DrawableObject.OriginPosition.Y;
            float targetPosition = referenceParent.ToLocalSpace(reference.ScreenSpaceDragPosition).Y - offsetFromReferenceOrigin;

            // Flip the vertical coordinate space when scrolling downwards
            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                targetPosition = targetPosition - referenceParent.DrawHeight;

            float movementDelta = targetPosition - reference.DrawableObject.Position.Y;

            foreach (var b in SelectedBlueprints.OfType<ManiaSelectionBlueprint>())
                b.DrawableObject.Y += movementDelta;
        }

        private void performDragMovement(MoveSelectionEvent moveEvent)
        {
            float delta = moveEvent.InstantDelta.Y;

            // When scrolling downwards the anchor position is at the bottom of the screen, however the movement event assumes the anchor is at the top of the screen.
            // This causes the delta to assume a positive hitobject position, and which can be corrected for by subtracting the parent height.
            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                delta -= moveEvent.Blueprint.DrawableObject.Parent.DrawHeight;

            foreach (var b in SelectedBlueprints)
            {
                var hitObject = b.DrawableObject;
                var objectParent = (HitObjectContainer)hitObject.Parent;

                // StartTime could be used to adjust the position if only one movement event was received per frame.
                // However this is not the case and ScrollingHitObjectContainer performs movement in UpdateAfterChildren() so the position must also be updated to be valid for further movement events
                hitObject.Y += delta;

                float targetPosition = hitObject.Position.Y;

                // The scrolling algorithm always assumes an anchor at the top of the screen, so the position must be flipped when scrolling downwards to reflect a top anchor
                if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                    targetPosition = -targetPosition;

                objectParent.Remove(hitObject);

                hitObject.HitObject.StartTime = scrollingInfo.Algorithm.TimeAt(targetPosition,
                    editorClock.CurrentTime,
                    scrollingInfo.TimeRange.Value,
                    objectParent.DrawHeight);

                objectParent.Add(hitObject);
            }
        }

        private void performColumnMovement(int lastColumn, MoveSelectionEvent moveEvent)
        {
            var currentColumn = composer.ColumnAt(moveEvent.ScreenSpacePosition);
            if (currentColumn == null)
                return;

            int columnDelta = currentColumn.Index - lastColumn;
            if (columnDelta == 0)
                return;

            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;

            foreach (var obj in SelectedHitObjects.OfType<ManiaHitObject>())
            {
                if (obj.Column < minColumn)
                    minColumn = obj.Column;
                if (obj.Column > maxColumn)
                    maxColumn = obj.Column;
            }

            columnDelta = MathHelper.Clamp(columnDelta, -minColumn, composer.TotalColumns - 1 - maxColumn);

            foreach (var obj in SelectedHitObjects.OfType<ManiaHitObject>())
                obj.Column += columnDelta;
        }
    }
}
