// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class SelectionBoxScaleHandle : SelectionBoxDragHandle
    {
        [Resolved]
        private SelectionBox selectionBox { get; set; } = null!;

        [Resolved]
        private SelectionScaleHandler? scaleHandler { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(10);
        }

        private Anchor originalAnchor;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (scaleHandler == null) return false;

            originalAnchor = Anchor;

            scaleHandler.Begin();
            return true;
        }

        private Vector2 rawScale;

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            if (scaleHandler == null) return;

            rawScale = convertDragEventToScaleMultiplier(e);

            applyScale(shouldLockAspectRatio: e.ShiftPressed);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (IsDragged && (e.Key == Key.ShiftLeft || e.Key == Key.ShiftRight))
            {
                applyScale(shouldLockAspectRatio: true);
                return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            base.OnKeyUp(e);

            if (IsDragged && (e.Key == Key.ShiftLeft || e.Key == Key.ShiftRight))
                applyScale(shouldLockAspectRatio: false);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            scaleHandler?.Commit();
        }

        private Vector2 convertDragEventToScaleMultiplier(DragEvent e)
        {
            Vector2 scale = e.MousePosition - e.MouseDownPosition;
            adjustScaleFromAnchor(ref scale);

            var surroundingQuad = scaleHandler!.OriginalSurroundingQuad!.Value;
            scale.X = Precision.AlmostEquals(surroundingQuad.Width, 0) ? 0 : scale.X / surroundingQuad.Width;
            scale.Y = Precision.AlmostEquals(surroundingQuad.Height, 0) ? 0 : scale.Y / surroundingQuad.Height;

            return scale + Vector2.One;
        }

        private void adjustScaleFromAnchor(ref Vector2 scale)
        {
            // cancel out scale in axes we don't care about (based on which drag handle was used).
            if ((originalAnchor & Anchor.x1) > 0) scale.X = 0;
            if ((originalAnchor & Anchor.y1) > 0) scale.Y = 0;

            // reverse the scale direction if dragging from top or left.
            if ((originalAnchor & Anchor.x0) > 0) scale.X = -scale.X;
            if ((originalAnchor & Anchor.y0) > 0) scale.Y = -scale.Y;
        }

        private void applyScale(bool shouldLockAspectRatio)
        {
            var newScale = shouldLockAspectRatio
                ? new Vector2((rawScale.X + rawScale.Y) * 0.5f)
                : rawScale;

            scaleHandler!.Update(newScale, getOriginPosition(), getAdjustAxis());
        }

        private Vector2 getOriginPosition()
        {
            var quad = scaleHandler!.OriginalSurroundingQuad!.Value;
            Vector2 origin = quad.TopLeft;

            if ((originalAnchor & Anchor.x0) > 0)
                origin.X += quad.Width;

            if ((originalAnchor & Anchor.y0) > 0)
                origin.Y += quad.Height;

            return origin;
        }

        private Axes getAdjustAxis()
        {
            switch (originalAnchor)
            {
                case Anchor.TopCentre:
                case Anchor.BottomCentre:
                    return Axes.Y;

                case Anchor.CentreLeft:
                case Anchor.CentreRight:
                    return Axes.X;

                default:
                    return Axes.Both;
            }
        }
    }
}
