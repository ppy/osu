// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// Represents the base appearance for UI controls of the <see cref="SelectionBox"/>,
    /// such as scale handles, rotation handles, buttons, etc...
    /// </summary>
    public abstract class SelectionBoxControl : CompositeDrawable
    {
        public event Action OperationStarted;
        public event Action OperationEnded;

        internal event Action HoverGained;
        internal event Action HoverLost;

        private Circle circle;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
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
            HoverGained?.Invoke();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            HoverLost?.Invoke();
            UpdateHoverState();
        }

        /// <summary>
        /// Whether this control is currently handling mouse down input.
        /// </summary>
        protected bool HandlingMouse { get; set; }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            HandlingMouse = true;
            UpdateHoverState();
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            HandlingMouse = false;
            UpdateHoverState();
            base.OnMouseUp(e);
        }

        protected virtual void UpdateHoverState()
        {
            circle.Colour = HandlingMouse ? Colours.GrayF : (IsHovered ? Colours.Red : Colours.YellowDark);
            this.ScaleTo(HandlingMouse || IsHovered ? 1.5f : 1, 100, Easing.OutQuint);
        }

        protected void OnOperationStarted() => OperationStarted?.Invoke();

        protected void OnOperationEnded() => OperationEnded?.Invoke();
    }
}
