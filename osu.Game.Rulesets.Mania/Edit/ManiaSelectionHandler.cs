// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaSelectionHandler : SelectionHandler
    {
        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private HitObjectComposer composer { get; set; }

        public override bool HandleMovement(MoveSelectionEvent moveEvent)
        {
            var maniaBlueprint = (ManiaSelectionBlueprint)moveEvent.Blueprint;
            int lastColumn = maniaBlueprint.DrawableObject.HitObject.Column;

            performColumnMovement(lastColumn, moveEvent);

            return true;
        }

        private void performColumnMovement(int lastColumn, MoveSelectionEvent moveEvent)
        {
            var maniaPlayfield = ((ManiaHitObjectComposer)composer).Playfield;

            var currentColumn = maniaPlayfield.GetColumnByPosition(moveEvent.ScreenSpacePosition);
            if (currentColumn == null)
                return;

            int columnDelta = currentColumn.Index - lastColumn;
            if (columnDelta == 0)
                return;

            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;

            foreach (var obj in EditorBeatmap.SelectedHitObjects.OfType<ManiaHitObject>())
            {
                if (obj.Column < minColumn)
                    minColumn = obj.Column;
                if (obj.Column > maxColumn)
                    maxColumn = obj.Column;
            }

            columnDelta = Math.Clamp(columnDelta, -minColumn, maniaPlayfield.TotalColumns - 1 - maxColumn);

            foreach (var obj in EditorBeatmap.SelectedHitObjects.OfType<ManiaHitObject>())
                obj.Column += columnDelta;
        }
    }
}
