// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A drag "handle" which shares the visual appearance but behaves more like a clickable button.
    /// </summary>
    public sealed class SelectionBoxDragHandleButton : SelectionBoxDragHandle, IHasTooltip
    {
        private SpriteIcon icon;

        private readonly IconUsage iconUsage;

        public Action Action;

        public SelectionBoxDragHandleButton(IconUsage iconUsage, string tooltip)
        {
            this.iconUsage = iconUsage;

            TooltipText = tooltip;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size *= 2;
            AddInternal(icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Icon = iconUsage,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override bool OnClick(ClickEvent e)
        {
            OperationStarted?.Invoke();
            Action?.Invoke();
            OperationEnded?.Invoke();
            return true;
        }

        protected override void UpdateHoverState()
        {
            base.UpdateHoverState();
            icon.Colour = !HandlingMouse && IsHovered ? Color4.White : Color4.Black;
        }

        public string TooltipText { get; }
    }
}
