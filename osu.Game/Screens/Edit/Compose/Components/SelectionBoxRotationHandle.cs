// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class SelectionBoxRotationHandle : SelectionBoxDragHandle, IHasTooltip
    {
        public LocalisableString TooltipText { get; private set; }

        private SpriteIcon icon = null!;

        private const float snap_step = 15;

        private readonly Bindable<float?> cumulativeRotation = new Bindable<float?>();

        [Resolved]
        private SelectionBox selectionBox { get; set; } = null!;

        [Resolved]
        private SelectionRotationHandler? rotationHandler { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(15f);
            AddInternal(icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.Redo,
                Scale = new Vector2
                {
                    X = Anchor.HasFlag(Anchor.x0) ? 1f : -1f,
                    Y = Anchor.HasFlag(Anchor.y0) ? 1f : -1f
                }
            });
        }

        protected override void UpdateHoverState()
        {
            base.UpdateHoverState();
            icon.FadeColour(!IsHeld && IsHovered ? Color4.White : Color4.Black, TRANSFORM_DURATION, Easing.OutQuint);
        }

        private float rawCumulativeRotation;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (rotationHandler == null) return false;

            if (rotationHandler.OperationInProgress.Value)
                return false;

            rotationHandler.Begin();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            if (rotationHandler == null || !rotationHandler.OperationInProgress.Value) return;

            rawCumulativeRotation += convertDragEventToAngleOfRotation(e);

            applyRotation(shouldSnap: e.ShiftPressed);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (IsDragged && (e.Key == Key.ShiftLeft || e.Key == Key.ShiftRight))
            {
                applyRotation(shouldSnap: true);
                return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            base.OnKeyUp(e);

            if (IsDragged && (e.Key == Key.ShiftLeft || e.Key == Key.ShiftRight))
                applyRotation(shouldSnap: false);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            rotationHandler?.Commit();
            UpdateHoverState();

            cumulativeRotation.Value = null;
            rawCumulativeRotation = 0;
            TooltipText = default;
        }

        private float convertDragEventToAngleOfRotation(DragEvent e)
        {
            // Adjust coordinate system to the center of the selection
            Vector2 center = selectionBox.ToLocalSpace(rotationHandler!.ToScreenSpace(rotationHandler!.DefaultOrigin!.Value));

            float startAngle = MathF.Atan2(e.LastMousePosition.Y - center.Y, e.LastMousePosition.X - center.X);
            float endAngle = MathF.Atan2(e.MousePosition.Y - center.Y, e.MousePosition.X - center.X);

            return (endAngle - startAngle) * 180 / MathF.PI;
        }

        private void applyRotation(bool shouldSnap)
        {
            float newRotation = shouldSnap ? snap(rawCumulativeRotation, snap_step) : MathF.Round(rawCumulativeRotation);
            newRotation = ((newRotation + 360 + 180) % 360) - 180;
            if (MathF.Abs(newRotation) == 180)
                newRotation = 180;

            cumulativeRotation.Value = newRotation;

            rotationHandler?.Update(newRotation);
            TooltipText = shouldSnap ? EditorStrings.RotationSnapped(newRotation) : EditorStrings.RotationUnsnapped(newRotation);
        }

        private float snap(float value, float step) => MathF.Round(value / step) * step;
    }
}
