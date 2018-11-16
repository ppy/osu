// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaSelectionBox : SelectionBox
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

        public override void HandleDrag(DragEvent dragEvent)
        {
            foreach (var blueprint in SelectedBlueprints)
            {
                var hitObject = blueprint.HitObject;

                var objectParent = hitObject.Parent;

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

                hitObject.HitObject.StartTime = scrollingInfo.Algorithm.TimeAt(targetPosition,
                    editorClock.CurrentTime,
                    scrollingInfo.TimeRange.Value,
                    objectParent.DrawHeight);
            }

            adjustColumn(dragEvent);
        }

        private void adjustColumn(DragEvent dragEvent)
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
