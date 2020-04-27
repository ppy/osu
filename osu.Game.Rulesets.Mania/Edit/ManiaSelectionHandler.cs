// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;

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

        public override bool HandleMovement(MoveSelectionEvent moveEvent)
        {
            var maniaBlueprint = (ManiaSelectionBlueprint)moveEvent.Blueprint;
            int lastColumn = maniaBlueprint.DrawableObject.HitObject.Column;

            performDragMovement(moveEvent);
            performColumnMovement(lastColumn, moveEvent);

            return true;
        }

        private void performDragMovement(MoveSelectionEvent moveEvent)
        {
            float delta = moveEvent.InstantDelta.Y;

            foreach (var selectionBlueprint in SelectedBlueprints)
            {
                var b = (OverlaySelectionBlueprint)selectionBlueprint;

                var hitObject = b.DrawableObject;
                var objectParent = (HitObjectContainer)hitObject.Parent;

                // We receive multiple movement events per frame such that we can't rely on updating the start time
                // since the scrolling hitobject container requires at least one update frame to update the position.
                // However the position needs to be valid for future movement events to calculate the correct deltas.
                hitObject.Y += delta;

                float targetPosition = hitObject.Position.Y;

                if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                {
                    // When scrolling downwards, the position is _negative_ when the object's start time is after the current time (e.g. in the middle of the stage).
                    // However all scrolling algorithms upwards scrolling, meaning that a positive (inverse) position is expected in the same scenario.
                    targetPosition = -targetPosition;
                }

                hitObject.HitObject.StartTime = scrollingInfo.Algorithm.TimeAt(targetPosition,
                    editorClock.CurrentTime,
                    scrollingInfo.TimeRange.Value,
                    objectParent.DrawHeight);
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

            columnDelta = Math.Clamp(columnDelta, -minColumn, composer.TotalColumns - 1 - maxColumn);

            foreach (var obj in SelectedHitObjects.OfType<ManiaHitObject>())
                obj.Column += columnDelta;
        }
    }
}
