// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class GridFromPointsTool : Drawable
    {
        [Resolved]
        private OsuGridToolboxGroup gridToolboxGroup { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private IPositionSnapProvider? snapProvider { get; set; }

        public bool IsPlacing { get; private set; }

        private Vector2? startPosition;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gridToolboxGroup.GridFromPointsClicked += BeginPlacement;
        }

        public void BeginPlacement()
        {
            IsPlacing = true;
            startPosition = null;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!IsPlacing)
                return base.OnMouseDown(e);

            var pos = snappedLocalPosition(e);

            if (!startPosition.HasValue)
                startPosition = pos;
            else
            {
                gridToolboxGroup.SetGridFromPoints(startPosition.Value, pos);
                IsPlacing = false;
            }

            if (e.Button == MouseButton.Right)
                IsPlacing = false;

            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!IsPlacing)
                return base.OnMouseMove(e);

            var pos = snappedLocalPosition(e);

            if (!startPosition.HasValue)
                gridToolboxGroup.StartPosition.Value = pos;
            else
                gridToolboxGroup.SetGridFromPoints(startPosition.Value, pos);

            return true;
        }

        private Vector2 snappedLocalPosition(UIEvent e)
        {
            return ToLocalSpace(snapProvider?.FindSnappedPositionAndTime(e.ScreenSpaceMousePosition, ~SnapType.GlobalGrids).ScreenSpacePosition ?? e.ScreenSpaceMousePosition);
        }
    }
}
