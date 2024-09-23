// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    public partial class GridPlacementBlueprint : PlacementBlueprint
    {
        [Resolved]
        private HitObjectComposer? hitObjectComposer { get; set; }

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
            {
                gridToolboxGroup.StartPosition.Value = originalOrigin;
                gridToolboxGroup.Spacing.Value = originalSpacing;
                gridToolboxGroup.GridLinesRotation.Value = originalRotation;
            }

            base.EndPlacement(commit);

            // You typically only place the grid once, so we switch back to the select tool after placement.
            if (commit && hitObjectComposer is OsuHitObjectComposer osuHitObjectComposer)
                osuHitObjectComposer.SetSelectTool();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    EndPlacement(true);
                    return true;

                case MouseButton.Left:
                    switch (PlacementActive)
                    {
                        case PlacementState.Waiting:
                            BeginPlacement(true);
                            return true;

                        case PlacementState.Active:
                            EndPlacement(true);
                            return true;
                    }

                    break;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            var pos = ToLocalSpace(result.ScreenSpacePosition);

            if (PlacementActive != PlacementState.Active)
                gridToolboxGroup.StartPosition.Value = pos;
            else
                gridToolboxGroup.SetGridFromPoints(gridToolboxGroup.StartPosition.Value, pos);
        }
    }
}
