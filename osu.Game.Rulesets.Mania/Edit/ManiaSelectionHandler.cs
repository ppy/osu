// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
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

        public override void HandleDrag(SelectionBlueprint blueprint, DragEvent dragEvent)
        {
            adjustOrigins((ManiaSelectionBlueprint)blueprint);
            performDragMovement(dragEvent);
            performColumnMovement(dragEvent);

            base.HandleDrag(blueprint, dragEvent);
        }

        /// <summary>
        /// Ensures that the position of hitobjects remains centred to the mouse position.
        /// E.g. The hitobject position will change if the editor scrolls while a hitobject is dragged.
        /// </summary>
        /// <param name="reference">The <see cref="ManiaSelectionBlueprint"/> that received the drag event.</param>
        private void adjustOrigins(ManiaSelectionBlueprint reference)
        {
            var referenceParent = (HitObjectContainer)reference.HitObject.Parent;

            float offsetFromReferenceOrigin = reference.DragPosition.Y - reference.HitObject.OriginPosition.Y;
            float targetPosition = referenceParent.ToLocalSpace(reference.ScreenSpaceDragPosition).Y - offsetFromReferenceOrigin;

            // Flip the vertical coordinate space when scrolling downwards
            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                targetPosition = targetPosition - referenceParent.DrawHeight;

            float movementDelta = targetPosition - reference.HitObject.Position.Y;

            foreach (var b in SelectedBlueprints.OfType<ManiaSelectionBlueprint>())
                b.HitObject.Y += movementDelta;
        }

        private void performDragMovement(DragEvent dragEvent)
        {
            foreach (var b in SelectedBlueprints)
            {
                var hitObject = b.HitObject;

                var objectParent = (HitObjectContainer)hitObject.Parent;

                // Using the hitobject position is required since AdjustPosition can be invoked multiple times per frame
                // without the position having been updated by the parenting ScrollingHitObjectContainer
                hitObject.Y += dragEvent.Delta.Y;

                float targetPosition;

                // If we're scrolling downwards, a position of 0 is actually further away from the hit target
                // so we need to flip the vertical coordinate in the hitobject container's space
                if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                    targetPosition = -hitObject.Position.Y;
                else
                    targetPosition = hitObject.Position.Y;

                objectParent.Remove(hitObject);

                hitObject.HitObject.StartTime = scrollingInfo.Algorithm.TimeAt(targetPosition,
                    editorClock.CurrentTime,
                    scrollingInfo.TimeRange.Value,
                    objectParent.DrawHeight);

                objectParent.Add(hitObject);
            }
        }

        private void performColumnMovement(DragEvent dragEvent)
        {
            var lastColumn = composer.ColumnAt(dragEvent.ScreenSpaceLastMousePosition);
            var currentColumn = composer.ColumnAt(dragEvent.ScreenSpaceMousePosition);
            if (lastColumn == null || currentColumn == null)
                return;

            int columnDelta = currentColumn.Index - lastColumn.Index;
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
