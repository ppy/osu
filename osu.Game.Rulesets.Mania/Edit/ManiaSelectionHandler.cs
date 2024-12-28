// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Mania.Edit
{
    public partial class ManiaSelectionHandler : EditorSelectionHandler
    {
        [Resolved]
        private HitObjectComposer composer { get; set; } = null!;

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            var selectedObjects = SelectedItems.OfType<ManiaHitObject>().ToArray();

            SelectionBox.CanFlipX = canFlipX(selectedObjects);
            SelectionBox.CanFlipY = canFlipY(selectedObjects);
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var hitObjectBlueprint = (HitObjectSelectionBlueprint)moveEvent.Blueprint;
            int lastColumn = ((ManiaHitObject)hitObjectBlueprint.Item).Column;

            performColumnMovement(lastColumn, moveEvent);

            return true;
        }

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            var selectedObjects = SelectedItems.OfType<ManiaHitObject>().ToArray();
            var maniaPlayfield = ((ManiaHitObjectComposer)composer).Playfield;

            if (selectedObjects.Length == 0)
                return false;

            switch (direction)
            {
                case Direction.Horizontal:
                    if (!canFlipX(selectedObjects))
                        return false;

                    int firstColumn = flipOverOrigin ? 0 : selectedObjects.Min(ho => ho.Column);
                    int lastColumn = flipOverOrigin ? (int)EditorBeatmap.BeatmapInfo.Difficulty.CircleSize - 1 : selectedObjects.Max(ho => ho.Column);

                    performOnSelection(maniaObject =>
                    {
                        maniaPlayfield.Remove(maniaObject);
                        maniaObject.Column = firstColumn + (lastColumn - maniaObject.Column);
                        maniaPlayfield.Add(maniaObject);
                    });

                    return true;

                case Direction.Vertical:
                    if (!canFlipY(selectedObjects))
                        return false;

                    double selectionStartTime = selectedObjects.Min(ho => ho.StartTime);
                    double selectionEndTime = selectedObjects.Max(ho => ho.GetEndTime());

                    performOnSelection(hitObject =>
                    {
                        hitObject.StartTime = selectionStartTime + (selectionEndTime - hitObject.GetEndTime());
                    });

                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Cannot flip over the supplied direction.");
            }
        }

        private static bool canFlipX(ManiaHitObject[] selectedObjects)
            => selectedObjects.Select(ho => ho.Column).Distinct().Count() > 1;

        private static bool canFlipY(ManiaHitObject[] selectedObjects)
            => selectedObjects.Length > 1 && selectedObjects.Min(ho => ho.StartTime) < selectedObjects.Max(ho => ho.GetEndTime());

        private void performColumnMovement(int lastColumn, MoveSelectionEvent<HitObject> moveEvent)
        {
            var maniaPlayfield = ((ManiaHitObjectComposer)composer).Playfield;

            var currentColumn = maniaPlayfield.GetColumnByPosition(moveEvent.Blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta);
            if (currentColumn == null)
                return;

            int columnDelta = currentColumn.Index - lastColumn;
            if (columnDelta == 0)
                return;

            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;

            var selectedObjects = EditorBeatmap.SelectedHitObjects.OfType<ManiaHitObject>().ToArray();

            // find min/max in an initial pass before actually performing the movement.
            foreach (var obj in selectedObjects)
            {
                if (obj.Column < minColumn)
                    minColumn = obj.Column;
                if (obj.Column > maxColumn)
                    maxColumn = obj.Column;
            }

            columnDelta = Math.Clamp(columnDelta, -minColumn, maniaPlayfield.TotalColumns - 1 - maxColumn);

            performOnSelection(h =>
            {
                maniaPlayfield.Remove(h);
                h.Column += columnDelta;
                maniaPlayfield.Add(h);
            });
        }

        private void performOnSelection(Action<ManiaHitObject> action)
        {
            var selectedObjects = EditorBeatmap.SelectedHitObjects.OfType<ManiaHitObject>().ToArray();

            EditorBeatmap.PerformOnSelection(h => action.Invoke((ManiaHitObject)h));

            // `HitObjectUsageEventBuffer`'s usage transferal flows and the playfield's `SetKeepAlive()` functionality do not combine well with mania's usage patterns,
            // leading to selections being sometimes partially dropped if some of the objects being moved are off screen
            // (check blame for detailed explanation).
            // thus, ensure that selection is preserved manually.
            EditorBeatmap.SelectedHitObjects.Clear();
            EditorBeatmap.SelectedHitObjects.AddRange(selectedObjects);
        }
    }
}
