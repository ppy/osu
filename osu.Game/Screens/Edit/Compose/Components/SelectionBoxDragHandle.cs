// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract class SelectionBoxDragHandle : SelectionBoxControl
    {
        public Action<DragEvent> HandleDrag { get; set; }

        protected override bool OnDragStart(DragStartEvent e)
        {
            OnOperationStarted();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            HandleDrag?.Invoke(e);
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            OnOperationEnded();

            UpdateHoverState();
            base.OnDragEnd(e);
        }

        #region Internal events for SelectionBoxDragHandleContainer

        internal event Action HoverGained;
        internal event Action HoverLost;

        protected override bool OnHover(HoverEvent e)
        {
            bool result = base.OnHover(e);
            HoverGained?.Invoke();
            return result;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            HoverLost?.Invoke();
        }

        #endregion
    }
}
