// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaSelectionHandler : EditorSelectionHandler
    {
        [Resolved]
        private HitObjectComposer composer { get; set; }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var hitObjectBlueprint = (HitObjectSelectionBlueprint)moveEvent.Blueprint;
            int lastColumn = ((ManiaHitObject)hitObjectBlueprint.Item).Column;

            performColumnMovement(lastColumn, moveEvent);

            return true;
        }

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

            // find min/max in an initial pass before actually performing the movement.
            foreach (var obj in EditorBeatmap.SelectedHitObjects.OfType<ManiaHitObject>())
            {
                if (obj.Column < minColumn)
                    minColumn = obj.Column;
                if (obj.Column > maxColumn)
                    maxColumn = obj.Column;
            }

            columnDelta = Math.Clamp(columnDelta, -minColumn, maniaPlayfield.TotalColumns - 1 - maxColumn);

            EditorBeatmap.PerformOnSelection(h =>
            {
                maniaPlayfield.Remove(h);
                ((ManiaHitObject)h).Column += columnDelta;
                maniaPlayfield.Add(h);
            });
        }
    }
}
