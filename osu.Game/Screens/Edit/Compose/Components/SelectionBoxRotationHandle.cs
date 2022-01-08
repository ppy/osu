// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBoxRotationHandle : SelectionBoxDragHandle, IHasTooltip
    {
        public Action<float> HandleRotate { get; set; }

        public LocalisableString TooltipText { get; private set; }

        private SpriteIcon icon;

        private readonly Bindable<float?> cumulativeRotation = new Bindable<float?>();

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

            float instantaneousAngle = convertDragEventToAngleOfRotation(e);
            cumulativeRotation.Value += instantaneousAngle;

            if (cumulativeRotation.Value < -180)
                cumulativeRotation.Value += 360;
            else if (cumulativeRotation.Value > 180)
                cumulativeRotation.Value -= 360;

            HandleRotate?.Invoke(instantaneousAngle);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            cumulativeRotation.Value = null;
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
            TooltipText = cumulativeRotation.Value?.ToLocalisableString("0.0°") ?? default(LocalisableString);
        }
    }
}
