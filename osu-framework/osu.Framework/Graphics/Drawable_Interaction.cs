//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using OpenTK.Input;
using OpenTK;
using osu.Framework.Lists;
using osu.Framework.Input;

namespace osu.Framework.Graphics
{
    public partial class Drawable : IDisposable, IHasLifetime
    {
        /// <summary>
        /// Find the first parent InputManager which this drawable is contained by.
        /// </summary>
        private InputManager ourInputManager => this as InputManager ?? Parent?.ourInputManager;

        public bool TriggerHover(InputState state)
        {
            return OnHover(state);
        }

        protected virtual bool OnHover(InputState state)
        {
            return false;
        }

        internal void TriggerHoverLost(InputState state)
        {
            OnHoverLost(state);
        }

        protected virtual void OnHoverLost(InputState state)
        {
        }

        public bool TriggerMouseDown(InputState state = null, MouseDownEventArgs args = null) => OnMouseDown(state, args);

        protected virtual bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            return false;
        }

        public bool TriggerMouseUp(InputState state = null, MouseUpEventArgs args = null) => OnMouseUp(state, args);

        protected virtual bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            return false;
        }

        public bool TriggerClick(InputState state = null) => OnClick(state);
        protected virtual bool OnClick(InputState state)
        {
            return false;
        }

        public bool TriggerDoubleClick(InputState state) => OnDoubleClick(state);
        protected virtual bool OnDoubleClick(InputState state)
        {
            return false;
        }

        public bool TriggerDragStart(InputState state) => OnDragStart(state);
        protected virtual bool OnDragStart(InputState state)
        {
            return false;
        }

        public bool TriggerDrag(InputState state) => OnDrag(state);
        protected virtual bool OnDrag(InputState state)
        {
            return false;
        }

        public bool TriggerDragEnd(InputState state) => OnDragEnd(state);
        protected virtual bool OnDragEnd(InputState state)
        {
            return false;
        }

        public bool TriggerWheelUp(InputState state) => OnWheelUp(state);
        protected virtual bool OnWheelUp(InputState state)
        {
            return false;
        }

        public bool TriggerWheelDown(InputState state) => OnWheelDown(state);
        protected virtual bool OnWheelDown(InputState state)
        {
            return false;
        }

        /// <summary>
        /// Focuses this drawable.
        /// </summary>
        /// <param name="state">The input state.</param>
        /// <param name="checkCanFocus">Whether we should check this Drawable's OnFocus returns true before actually providing focus.</param>
        public bool TriggerFocus(InputState state = null, bool checkCanFocus = false)
        {
            if (HasFocus)
                return true;

            if (checkCanFocus & !OnFocus(state))
                return false;

            ourInputManager?.ChangeFocus(this);

            return true;
        }

        protected virtual bool OnFocus(InputState state)
        {
            return false;
        }

        /// <summary>
        /// Unfocuses this drawable.
        /// </summary>
        /// <param name="state">The input state.</param>
        internal void TriggerFocusLost(InputState state = null, bool isCallback = false)
        {
            if (!HasFocus)
                return;

            if (state == null)
                state = new InputState();

            if (!isCallback) ourInputManager.ChangeFocus(null);
            OnFocusLost(state);
        }

        protected virtual void OnFocusLost(InputState state)
        {
        }

        public bool TriggerKeyDown(InputState state, KeyDownEventArgs args) => OnKeyDown(state, args);
        protected virtual bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            return false;
        }

        public bool TriggerKeyUp(InputState state, KeyUpEventArgs args) => OnKeyUp(state, args);
        protected virtual bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            return false;
        }

        public bool TriggerMouseMove(InputState state) => OnMouseMove(state);
        protected virtual bool OnMouseMove(InputState state)
        {
            return false;
        }

        public bool HandleInput = true;

        internal virtual bool HasFocus => ourInputManager?.FocusedDrawable == this;

        internal bool Hovering;

        /// <summary>
        /// Sometimes we need to know the position of the mouse inside the drawable.
        /// </summary>
        /// <param name="screenSpacePos">A position in screen space (user input device).</param>
        /// <returns>The relative (0..1) position inside (or outside) the drawable.</returns>
        internal virtual Vector2? GetContainedPosition(Vector2 screenSpacePos)
        {
            return ScreenSpaceInputQuad.Contains(screenSpacePos);
        }

        public virtual Vector2 GetLocalPosition(Vector2 screenSpacePos)
        {
            return screenSpacePos * DrawInfo.MatrixInverse;
        }

        internal virtual bool Contains(Vector2 screenSpacePos)
        {
            return ScreenSpaceInputQuad.Contains(screenSpacePos).HasValue;
        }
    }

    public class KeyDownEventArgs : EventArgs
    {
        public Key Key;
        public bool Repeat;
    }

    public class MouseUpEventArgs : MouseEventArgs { }
    public class MouseDownEventArgs : MouseEventArgs { }

    public class MouseEventArgs : EventArgs
    {
        public MouseButton Button;
    }

    public class KeyUpEventArgs : EventArgs
    {
        public Key Key;
    }

    public delegate bool MouseEventHandlerDelegate(object sender, InputState state);
    internal delegate bool KeyDownEventHandlerDelegate(object sender, KeyDownEventArgs e, InputState state);
    internal delegate bool KeyUpEventHandlerDelegate(object sender, KeyUpEventArgs e, InputState state);
}
