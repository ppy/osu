// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBoxRotationHandle : SelectionBoxDragHandle, IHasTooltip
    {
        public Action<float> HandleRotate { get; set; }

        public LocalisableString TooltipText { get; private set; }

        private SpriteIcon icon;

        private readonly Bindable<float?> cumulativeRotation = new Bindable<float?>();

        private bool isSnapping;

        [Resolved]
        private SelectionBox selectionBox { get; set; }

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
                    X = Anchor.HasFlagFast(Anchor.x0) ? 1f : -1f,
                    Y = Anchor.HasFlagFast(Anchor.y0) ? 1f : -1f
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            cumulativeRotation.BindValueChanged(_ => updateTooltipText(), true);
        }

        protected override void UpdateHoverState()
        {
            base.UpdateHoverState();
            icon.FadeColour(!IsHeld && IsHovered ? Color4.White : Color4.Black, TRANSFORM_DURATION, Easing.OutQuint);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            bool handle = base.OnDragStart(e);
            if (handle)
                cumulativeRotation.Value = 0;
            return handle;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            float oldRoundedCumulativeRotationValue = roundToNearestFifths(cumulativeRotation.Value ?? 0);

            float instantaneousAngle = convertDragEventToAngleOfRotation(e);
            cumulativeRotation.Value += instantaneousAngle;

            if (cumulativeRotation.Value < -180)
                cumulativeRotation.Value += 360;
            else if (cumulativeRotation.Value > 180)
                cumulativeRotation.Value -= 360;

            if (isSnapping)
            {
                float roundedCumulativeRotation = roundToNearestFifths(cumulativeRotation.Value ?? 0);
                instantaneousAngle = roundedCumulativeRotation - oldRoundedCumulativeRotationValue;
            }

            HandleRotate?.Invoke(instantaneousAngle);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            cumulativeRotation.Value = null;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            isSnapping = e.Key == Key.ControlLeft;

            // Make sure that the starting point is rounded to the nearest fifths
            if (cumulativeRotation.Value != null)
            {
                HandleRotate?.Invoke(roundToNearestFifths(cumulativeRotation.Value ?? 0) - (cumulativeRotation.Value ?? 0));
                updateTooltipText();
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == Key.ControlLeft)
                isSnapping = false;

            base.OnKeyUp(e);
        }

        private float convertDragEventToAngleOfRotation(DragEvent e)
        {
            // Adjust coordinate system to the center of SelectionBox
            float startAngle = MathF.Atan2(e.LastMousePosition.Y - selectionBox.DrawHeight / 2, e.LastMousePosition.X - selectionBox.DrawWidth / 2);
            float endAngle = MathF.Atan2(e.MousePosition.Y - selectionBox.DrawHeight / 2, e.MousePosition.X - selectionBox.DrawWidth / 2);

            return (endAngle - startAngle) * 180 / MathF.PI;
        }

        private void updateTooltipText()
        {
            TooltipText = (isSnapping ? roundToNearestFifths(cumulativeRotation.Value ?? 0) : cumulativeRotation.Value)?.ToLocalisableString("0.0°") ?? default;
        }

        private float roundToNearestFifths(float angle) => MathF.Round((angle) / 5f) * 5f;
    }
}
