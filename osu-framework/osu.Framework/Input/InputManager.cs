//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Input;
using Rectangle = System.Drawing.Rectangle;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Lists;
using osu.Framework.Input.Handlers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Timing;

namespace osu.Framework.Input
{
    public class InputManager : LargeContainer
    {
        /// <summary>
        /// The initial delay before key repeat begins.
        /// </summary>
        private const int repeat_initial_delay = 250;

        /// <summary>
        /// Should we ignore this InputManager and use a parent-level implementation instead?
        /// </summary>
        public bool PassThrough;

        /// <summary>
        /// The delay between key repeats after the initial repeat.
        /// </summary>
        private const int repeat_tick_rate = 70;

        /// <summary>
        /// The maximum time between two clicks for a double-click to be considered.
        /// </summary>
        private const int double_click_time = 250;

        /// <summary>
        /// The distance that must be moved before a drag begins.
        /// </summary>
        private const float drag_start_distance = 5;

        /// <summary>
        /// The distance that can be moved between MouseDown and MouseUp to consider a click valid to take action on.
        /// </summary>
        private const float click_confirmation_distance = 10;

        /// <summary>
        /// The time of the last input action.
        /// </summary>
        public double LastActionTime;

        public Drawable FocusedDrawable;

        private readonly List<InputHandler> inputHandlers = new List<InputHandler>();

        private double lastClickTime;

        private double keyboardRepeatTime;

        private bool isDragging;

        private bool isValidClick;

        private ICursorInputHandler currentCursorHandler;

        /// <summary>
        /// The input state from the previous frame.
        /// </summary>
        private InputState inputState = new InputState();

        /// <summary>
        /// The sequential list in which to handle mouse input.
        /// </summary>
        private List<Drawable> mouseInputQueue = new List<Drawable>();

        /// <summary>
        /// The sequential list in which to handle keyboard input.
        /// </summary>
        private List<Drawable> keyboardInputQueue = new List<Drawable>();

        private Drawable draggingDrawable;
        private List<Drawable> hoveredDrawables = new List<Drawable>();
        private Drawable hoverHandledDrawable;

        internal void ChangeFocus(Drawable focus)
        {
            if (focus == FocusedDrawable) return;

            FocusedDrawable?.TriggerFocusLost(null, true);
            FocusedDrawable = focus;
        }

        protected override void Dispose(bool isDisposing)
        {
            foreach (InputHandler h in inputHandlers)
                h.Dispose();

            base.Dispose(isDisposing);
        }

        private void onResolutionChange(bool b)
        {
            foreach (InputHandler h in inputHandlers)
                h.OnResolutionChange();
        }

        protected override void Update()
        {
            inputState.Keyboard.LastState = null;
            inputState.Mouse.LastState = null;

            inputState = new InputState(inputState);

            UpdateKeyboardState(inputState);
            UpdateMouseState(inputState);

            updateInputQueues(inputState);

            if (!PassThrough)
            {
                updateHoverEvents(inputState);
                updateKeyboardEvents(inputState);
                updateMouseEvents(inputState);
            }

            base.Update();
        }

        private void updateInputQueues(InputState state)
        {
            keyboardInputQueue.Clear();
            mouseInputQueue.Clear();

            buildKeyboardInputQueue(this);
            buildMouseInputQueue(state, this);

            keyboardInputQueue.Reverse();
            mouseInputQueue.Reverse();
        }

        private void buildKeyboardInputQueue(Drawable current)
        {
            if (!current.HandleInput || !current.IsVisible)
                return;

            if (current != this)
            {
                //stop processing at any nested InputManagers
                if ((current as InputManager)?.PassThrough == false)
                    return;

                keyboardInputQueue.Add(current);
            }

            foreach (Drawable child in current.Children)
                buildKeyboardInputQueue(child);
        }

        private void buildMouseInputQueue(InputState state, Drawable current)
        {
            if (!current.HandleInput || !current.IsVisible)
                return;

            if (!current.Contains(state.Mouse.Position))
                return;

            if (current != this)
            {
                //stop processing at any nested InputManagers
                if ((current as InputManager)?.PassThrough == false)
                    return;

                mouseInputQueue.Add(current);
            }

            foreach (Drawable child in current.Children)
                buildMouseInputQueue(state, child);
        }

        private void updateHoverEvents(InputState state)
        {
            Drawable lastHoverHandledDrawable = hoverHandledDrawable;
            hoverHandledDrawable = null;

            List<Drawable> lastHoveredDrawables = new List<Drawable>(hoveredDrawables);
            hoveredDrawables.Clear();

            // Unconditionally unhover all that aren't directly hovered anymore
            List<Drawable> newlyUnhoveredDrawables = lastHoveredDrawables.Except(mouseInputQueue).ToList();
            foreach (Drawable d in newlyUnhoveredDrawables)
            {
                d.Hovering = false;
                d.TriggerHoverLost(state);
            }

            // Don't care about what's now explicitly unhovered
            lastHoveredDrawables = lastHoveredDrawables.Except(newlyUnhoveredDrawables).ToList();

            // lastHoveredDrawables now contain only drawables that were hovered in the previous frame
            // that may continue being hovered. We need to construct hoveredDrawables for the current frame
            foreach (Drawable d in mouseInputQueue)
            {
                hoveredDrawables.Add(d);
                lastHoveredDrawables.Remove(d);

                // Don't need to re-hover those that are already hovered
                if (d.Hovering)
                {
                    // Check if this drawable previously handled hover, and assume it would once more
                    if (d == lastHoverHandledDrawable)
                    {
                        hoverHandledDrawable = lastHoverHandledDrawable;
                        break;
                    }

                    continue;
                }

                d.Hovering = true;
                if (d.TriggerHover(state))
                {
                    hoverHandledDrawable = d;
                    break;
                }
            }

            // lastHoveredDrawables now contains only drawables that were hovered in the previous frame
            // but should no longer be hovered as a result of a drawable handling hover this frame
            foreach (Drawable d in lastHoveredDrawables)
            {
                d.Hovering = false;
                d.TriggerHoverLost(state);
            }
        }

        protected virtual void UpdateKeyboardState(InputState state)
        {
            if (PassThrough) return;

            List<Key> keys = new List<Key>();

            foreach (InputHandler h in inputHandlers)
            {
                if (!h.IsActive) continue;

                IKeyboardInputHandler kh = h as IKeyboardInputHandler;

                if (kh == null) continue;

                h.UpdateInput(currentCursorHandler == h);

                keys.AddRange(kh.PressedKeys);
            }

            state.Keyboard.Keys = new ReadOnlyList<Key>(keys);
        }

        protected virtual void UpdateMouseState(InputState state)
        {
            if (PassThrough) return;

            MouseState mouse = state.Mouse;

            currentCursorHandler = null;

            foreach (InputHandler h in inputHandlers)
            {
                if (!h.IsActive) continue;

                ICursorInputHandler ch = h as ICursorInputHandler;

                if (ch == null) continue;

                // Make first handler which is active the current handler. (Handlers are ordered by priority.)
                if (currentCursorHandler == null && ch.Position != null)
                    currentCursorHandler = ch;

                h.UpdateInput(currentCursorHandler == h);

                mouse.ButtonStates.ForEach(b =>
                {
                    switch (b.Button)
                    {
                        case MouseButton.Left:
                            b.State |= ch.Left ?? false;
                            break;
                        case MouseButton.Middle:
                            b.State |= ch.Middle ?? false;
                            break;
                        case MouseButton.Right:
                            b.State |= ch.Right ?? false;
                            break;
                        case MouseButton.Button1:
                            b.State |= ch.Back ?? false;
                            break;
                        case MouseButton.Button2:
                            b.State |= ch.Forward ?? false;
                            break;
                    }
                });

                mouse.WheelUp |= ch.WheelUp ?? false;
                mouse.WheelDown |= ch.WheelDown ?? false;
            }

            if (currentCursorHandler != null)
            {
                //convert InputHandler coordinates to native scren coordinates.
                Vector2 pos = currentCursorHandler.Position ?? Vector2.Zero;

                pos.X /= currentCursorHandler.Size.X;
                pos.Y /= currentCursorHandler.Size.Y;

                Quad q = ScreenSpaceInputQuad;

                mouse.Position = q.TopLeft + new Vector2(pos.X * q.Width, pos.Y * q.Height);
            }
            else
                mouse.Position = Vector2.Zero;
        }

        private void updateKeyboardEvents(InputState state)
        {
            KeyboardState keyboard = state.Keyboard;

            if (keyboard.Keys.Count == 0)
                keyboardRepeatTime = 0;
            else
                keyboardRepeatTime -= (Clock as FramedClock)?.ElapsedFrameTime ?? 0;

            keyboard.LastState?.Keys.ForEach(k =>
            {
                if (!keyboard.Keys.Contains(k))
                    handleKeyUp(state, k);
            });

            foreach (Key k in keyboard.Keys)
            {
                bool isModifier = k == Key.LControl || k == Key.RControl
                                  || k == Key.LAlt || k == Key.RAlt
                                  || k == Key.LShift || k == Key.RShift;

                if (isModifier)
                    continue;

                bool isRepetition = keyboard.LastState?.Keys.Contains(k) ?? false;

                LastActionTime = Time;

                if (isRepetition)
                {
                    if (keyboardRepeatTime <= 0)
                    {
                        keyboardRepeatTime += repeat_tick_rate;
                        handleKeyDown(state, k, true);
                    }
                }
                else
                {
                    keyboardRepeatTime = repeat_initial_delay;
                    handleKeyDown(state, k, false);
                }
            }
        }

        private void updateMouseEvents(InputState state)
        {
            MouseState mouse = state.Mouse;

            if (mouse.Position != mouse.LastState?.Position)
            {
                handleMouseMove(state);
                if (isDragging)
                    handleMouseDrag(state);
            }

            foreach (MouseState.ButtonState b in mouse.ButtonStates)
            {
                if (b.State != mouse.LastState?.ButtonStates.Find(c => c.Button == b.Button).State)
                {
                    if (b.State)
                        handleMouseDown(state, b.Button);
                    else
                        handleMouseUp(state, b.Button);
                }
            }

            if (mouse.WheelUp)
                handleWheelUp(state);

            if (mouse.WheelDown)
                handleWheelDown(state);

            if (mouse.HasMainButtonPressed)
            {
                if (mouse.LastState?.HasMainButtonPressed != true)
                {
                    //stuff which only happens once after the mousedown state
                    state.Mouse.PositionMouseDown = state.Mouse.Position;
                    LastActionTime = Time;
                    isValidClick = true;

                    if (Time - lastClickTime < double_click_time)
                    {
                        if (handleMouseDoubleClick(state))
                            //when we handle a double-click we want to block a normal click from firing.
                            isValidClick = false;

                        lastClickTime = 0;
                    }

                    lastClickTime = Time;
                }

                if (!isDragging && Vector2.Distance(mouse.PositionMouseDown ?? mouse.Position, mouse.Position) > drag_start_distance)
                {
                    isDragging = true;
                    handleMouseDrag(state);
                }

                if (isValidClick && Vector2.Distance(mouse.PositionMouseDown ?? mouse.Position, mouse.Position) > click_confirmation_distance)
                    isValidClick = false;
            }
            else if (mouse.LastState?.HasMainButtonPressed == true)
            {
                if (isValidClick)
                    handleMouseClick(state);

                mouse.PositionMouseDown = null;

                if (isDragging)
                {
                    isDragging = false;
                    handleMouseDragEnd(state);
                }
            }
        }

        private bool handleMouseDown(InputState state, MouseButton button)
        {
            MouseDownEventArgs args = new MouseDownEventArgs()
            {
                Button = button
            };

            return mouseInputQueue.Any(target => target.TriggerMouseDown(state, args));
        }

        private bool handleMouseUp(InputState state, MouseButton button)
        {
            MouseUpEventArgs args = new MouseUpEventArgs()
            {
                Button = button
            };

            return mouseInputQueue.Any(target => target.TriggerMouseUp(state, args));
        }

        private bool handleMouseMove(InputState state)
        {
            return mouseInputQueue.Any(target => target.TriggerMouseMove(state));
        }

        private bool handleMouseClick(InputState state)
        {
            if (mouseInputQueue.Any(target => target.TriggerClick(state) | target.TriggerFocus(state, true)))
                return true;

            FocusedDrawable?.TriggerFocusLost();
            return false;
        }

        private bool handleMouseDoubleClick(InputState state)
        {
            return mouseInputQueue.Any(target => target.TriggerDoubleClick(state));
        }

        private bool handleMouseDrag(InputState state)
        {
            //Once a drawable is dragged, it remains in a dragged state until the drag is finished.
            return draggingDrawable?.TriggerDrag(state) ?? mouseInputQueue.Any(target => handleMouseDragStart(state, target) || target.TriggerDrag(state));
        }

        private bool handleMouseDragStart(InputState state, Drawable target)
        {
            if (!target.TriggerDragStart(state)) return false;

            draggingDrawable = target;
            return true;
        }

        private bool handleMouseDragEnd(InputState state)
        {
            if (draggingDrawable == null)
                return false;

            bool result = draggingDrawable.TriggerDragEnd(state);
            draggingDrawable = null;

            return result;
        }

        private bool handleWheelUp(InputState state)
        {
            return mouseInputQueue.Any(target => target.TriggerWheelUp(state));
        }

        private bool handleWheelDown(InputState state)
        {
            return mouseInputQueue.Any(target => target.TriggerWheelDown(state));
        }

        private bool handleKeyDown(InputState state, Key key, bool repeat)
        {
            KeyDownEventArgs args = new KeyDownEventArgs
            {
                Key = key,
                Repeat = repeat
            };

            if (FocusedDrawable != null)
            {
                if (args.Key == Key.Escape)
                    FocusedDrawable.TriggerFocusLost(state);
                else if (FocusedDrawable.TriggerKeyDown(state, args))
                    return true;
            }

            return keyboardInputQueue.Any(target => target.TriggerKeyDown(state, args));
        }

        private bool handleKeyUp(InputState state, Key key)
        {
            KeyUpEventArgs args = new KeyUpEventArgs { Key = key };

            if (FocusedDrawable?.TriggerKeyUp(state, args) ?? false)
                return true;

            return keyboardInputQueue.Any(target => target.TriggerKeyUp(state, args));
        }

        public InputHandler GetHandler(Type handlerType)
        {
            return inputHandlers.Find(h => h.GetType() == handlerType);
        }

        protected bool AddHandler(InputHandler handler)
        {
            try
            {
                if (handler.Initialize())
                {
                    int index = inputHandlers.BinarySearch(handler, new InputHandlerComparer());
                    if (index < 0)
                    {
                        index = ~index;
                    }

                    inputHandlers.Insert(index, handler);

                    //set the initial position to the current OsuGame position.
                    (handler as ICursorInputHandler)?.SetPosition(inputState.Mouse.Position);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        internal void SetCursorHandlerPositions(Vector2 pos)
        {
            foreach (InputHandler h in inputHandlers)
            {
                // We don't want to set the position of the currently active cursor handler since it is the one controlling out position
                // in the first place. This is avoiding potential overhead with resetting its position. (Such as forcfully moving the windows cursor.)
                if (h != currentCursorHandler)
                    (h as ICursorInputHandler)?.SetPosition(pos);
            }
        }
    }

    public enum ConfineMouseMode
    {
        Never,
        Fullscreen,
        Always
    }
}
