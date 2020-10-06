// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBoxDragHandle : Container
    {
        public Action OperationStarted;
        public Action OperationEnded;

        public Action<DragEvent> HandleDrag { get; set; }

        private Circle circle;

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(10);
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                circle = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateHoverState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateHoverState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            UpdateHoverState();
        }

        protected bool HandlingMouse;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            HandlingMouse = true;
            UpdateHoverState();
            return true;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            OperationStarted?.Invoke();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            HandleDrag?.Invoke(e);
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            HandlingMouse = false;
            OperationEnded?.Invoke();

            UpdateHoverState();
            base.OnDragEnd(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            HandlingMouse = false;
            UpdateHoverState();
            base.OnMouseUp(e);
        }

        protected virtual void UpdateHoverState()
        {
            circle.Colour = HandlingMouse ? colours.GrayF : (IsHovered ? colours.Red : colours.YellowDark);
            this.ScaleTo(HandlingMouse || IsHovered ? 1.5f : 1, 100, Easing.OutQuint);
        }
    }
}
