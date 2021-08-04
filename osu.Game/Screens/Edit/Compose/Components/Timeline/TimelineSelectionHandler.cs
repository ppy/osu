// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineSelectionHandler : EditorSelectionHandler, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private Timeline timeline { get; set; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => timeline.ScreenSpaceDrawQuad.Contains(screenSpacePos);

        // for now we always allow movement. snapping is provided by the Timeline's "distance" snap implementation
        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent) => true;

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
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

        public void OnReleased(GlobalAction action)
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
    }
}
