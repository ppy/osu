// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    public partial class GridPlacementBlueprint : PlacementBlueprint
    {
        [Resolved]
        private OsuHitObjectComposer? hitObjectComposer { get; set; }

        private OsuGridToolboxGroup gridToolboxGroup = null!;
        private Vector2 originalOrigin;
        private float originalSpacing;
        private float originalRotation;

        [BackgroundDependencyLoader]
        private void load(OsuGridToolboxGroup gridToolboxGroup)
        {
            this.gridToolboxGroup = gridToolboxGroup;
            originalOrigin = gridToolboxGroup.StartPosition.Value;
            originalSpacing = gridToolboxGroup.Spacing.Value;
            originalRotation = gridToolboxGroup.GridLinesRotation.Value;
        }

        public override void EndPlacement(bool commit)
        {
            if (!commit && PlacementActive != PlacementState.Finished)
                resetGridState();

            base.EndPlacement(commit);

            // You typically only place the grid once, so we switch back to the last tool after placement.
            if (commit && hitObjectComposer is OsuHitObjectComposer osuHitObjectComposer)
                osuHitObjectComposer.SetLastTool();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                switch (PlacementActive)
                {
                    case PlacementState.Waiting:
                        BeginPlacement(true);
                        return true;

                    case PlacementState.Active:
                        EndPlacement(true);
                        return true;
                }
            }

            return base.OnClick(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
            {
                // Reset the grid to the default values.
                gridToolboxGroup.StartPosition.Value = gridToolboxGroup.StartPosition.Default;
                gridToolboxGroup.Spacing.Value = gridToolboxGroup.Spacing.Default;
                if (!gridToolboxGroup.GridLinesRotation.Disabled)
                    gridToolboxGroup.GridLinesRotation.Value = gridToolboxGroup.GridLinesRotation.Default;
                EndPlacement(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                BeginPlacement(true);
                return true;
            }

            return base.OnDragStart(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (PlacementActive == PlacementState.Active)
                EndPlacement(true);

            base.OnDragEnd(e);
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            if (State.Value == Visibility.Hidden)
                return new SnapResult(screenSpacePosition, fallbackTime);

            var result = hitObjectComposer?.TrySnapToNearbyObjects(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);

            var pos = ToLocalSpace(result.ScreenSpacePosition);

            if (PlacementActive != PlacementState.Active)
                gridToolboxGroup.StartPosition.Value = pos;
            else
            {
                // Default to the original spacing and rotation if the distance is too small.
                if (Vector2.Distance(gridToolboxGroup.StartPosition.Value, pos) < 2)
                {
                    gridToolboxGroup.Spacing.Value = originalSpacing;
                    if (!gridToolboxGroup.GridLinesRotation.Disabled)
                        gridToolboxGroup.GridLinesRotation.Value = originalRotation;
                }
                else
                {
                    gridToolboxGroup.SetGridFromPoints(gridToolboxGroup.StartPosition.Value, pos);
                }
            }

            return result;
        }

        protected override void PopOut()
        {
            base.PopOut();
            resetGridState();
        }

        private void resetGridState()
        {
            gridToolboxGroup.StartPosition.Value = originalOrigin;
            gridToolboxGroup.Spacing.Value = originalSpacing;
            if (!gridToolboxGroup.GridLinesRotation.Disabled)
                gridToolboxGroup.GridLinesRotation.Value = originalRotation;
        }
    }
}
