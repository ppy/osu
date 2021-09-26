// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineSelectionHandler : EditorSelectionHandler, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private Timeline timeline { get; set; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => timeline.ScreenSpaceDrawQuad.Contains(screenSpacePos);

        // for now we always allow movement. snapping is provided by the Timeline's "distance" snap implementation
        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent) => true;

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
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

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
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

        internal override bool MouseDownSelectionRequested(SelectionBlueprint<HitObject> blueprint, MouseButtonEvent e)
        {
            if (e.ShiftPressed && e.Button == MouseButton.Left && SelectedItems.Any())
            {
                handleRangeSelection(blueprint);
                return true;
            }

            return base.MouseDownSelectionRequested(blueprint, e);
        }

        private void handleRangeSelection(SelectionBlueprint<HitObject> blueprint)
        {
            var clickedObject = blueprint.Item;
            double rangeStart = clickedObject.StartTime;
            double rangeEnd = clickedObject.GetEndTime();

            foreach (var selectedObject in SelectedItems)
            {
                rangeStart = Math.Min(rangeStart, selectedObject.StartTime);
                rangeEnd = Math.Max(rangeEnd, selectedObject.GetEndTime());
            }

            EditorBeatmap.SelectedHitObjects.Clear();
            EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects.Where(obj => isInRange(obj, rangeStart, rangeEnd)));

            bool isInRange(HitObject hitObject, double start, double end)
                => hitObject.StartTime >= start && hitObject.GetEndTime() <= end;
        }
    }
}
