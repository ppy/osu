// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal partial class TimelineSelectionHandler : EditorSelectionHandler
    {
        // for now we always allow movement. snapping is provided by the Timeline's "distance" snap implementation
        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent) => true;

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorNudgeLeft:
                    nudgeSelection(-1);
                    return true;

                case GlobalAction.EditorNudgeRight:
                    nudgeSelection(1);
                    return true;
            }

            return base.OnPressed(e);
        }

        /// <summary>
        /// Nudge the current selection by the specified multiple of beat divisor lengths,
        /// based on the timing at the first object in the selection.
        /// </summary>
        /// <param name="amount">The direction and count of beat divisor lengths to adjust.</param>
        private void nudgeSelection(int amount)
        {
            var selected = EditorBeatmap.SelectedHitObjects;

            if (selected.Count == 0)
                return;

            var timingPoint = EditorBeatmap.ControlPointInfo.TimingPointAt(selected.First().StartTime);
            double adjustment = timingPoint.BeatLength / EditorBeatmap.BeatDivisor * amount;

            EditorBeatmap.PerformOnSelection(h =>
            {
                h.StartTime += adjustment;
                EditorBeatmap.Update(h);
            });
        }

        /// <summary>
        /// The "pivot" object, used in range selection mode.
        /// When in range selection, the range to select is determined by the pivot object
        /// (last existing object interacted with prior to holding down Shift)
        /// and by the object clicked last when Shift was pressed.
        /// </summary>
        [CanBeNull]
        private HitObject pivot;

        internal override bool MouseDownSelectionRequested(SelectionBlueprint<HitObject> blueprint, MouseButtonEvent e)
        {
            if (e.ShiftPressed && e.Button == MouseButton.Left && pivot != null)
            {
                handleRangeSelection(blueprint, e.ControlPressed);
                return true;
            }

            bool result = base.MouseDownSelectionRequested(blueprint, e);
            // ensure that the object wasn't removed by the base implementation before making it the new pivot.
            if (EditorBeatmap.HitObjects.Contains(blueprint.Item))
                pivot = blueprint.Item;
            return result;
        }

        /// <summary>
        /// Handles a request for range selection (triggered when Shift is held down).
        /// </summary>
        /// <param name="blueprint">The blueprint which was clicked in range selection mode.</param>
        /// <param name="cumulative">
        /// Whether the selection should be cumulative.
        /// In cumulative mode, consecutive range selections will shift the pivot (which usually stays fixed for the duration of a range selection)
        /// and will never deselect an object that was previously selected.
        /// </param>
        private void handleRangeSelection(SelectionBlueprint<HitObject> blueprint, bool cumulative)
        {
            var clickedObject = blueprint.Item;

            Debug.Assert(pivot != null);

            double rangeStart = Math.Min(clickedObject.StartTime, pivot.StartTime);
            double rangeEnd = Math.Max(clickedObject.GetEndTime(), pivot.GetEndTime());

            var newSelection = new HashSet<HitObject>(EditorBeatmap.HitObjects.Where(obj => isInRange(obj, rangeStart, rangeEnd)));

            if (cumulative)
            {
                pivot = clickedObject;
                newSelection.UnionWith(EditorBeatmap.SelectedHitObjects);
            }

            EditorBeatmap.SelectedHitObjects.Clear();
            EditorBeatmap.SelectedHitObjects.AddRange(newSelection);

            static bool isInRange(HitObject hitObject, double start, double end)
                => hitObject.StartTime >= start && hitObject.GetEndTime() <= end;
        }
    }
}
