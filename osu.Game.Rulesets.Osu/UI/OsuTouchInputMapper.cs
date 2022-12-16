// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuTouchInputMapper : Drawable
    {
        /// <summary>
        /// The max amount of touches that we handle.
        /// </summary>
        private readonly BindableInt maxHandledTapTouches = new BindableInt(2);

        /// <summary>
        /// The max amount of touches that we handle. Decremented by one.
        /// </summary>
        private int maxHandledTapTouchesDecremented;

        /// <summary>
        /// This is our parent <see cref="osuInputManager"/>.
        /// </summary>
        private readonly OsuInputManager osuInputManager;

        /// <summary>
        /// This is our <see cref="keyBindingContainer"/>. We trigger <see cref="OsuAction"/>s  with it.
        /// </summary>
        private KeyBindingContainer<OsuAction> keyBindingContainer => osuInputManager.KeyBindingContainer;

        /// <summary>
        /// This is a dictionary tracking all the active <see cref="TouchSource"/>s and the <see cref="OsuAction"/> that it triggered.
        /// </summary>
        private readonly Dictionary<TouchSource, OsuAction> triggeredActions = new Dictionary<TouchSource, OsuAction>();

        /// <summary>
        /// Tracks how many touch sources are currently active.
        /// </summary>
        public int ActiveTapTouchesCount => triggeredActions.Count;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            maxHandledTapTouches.BindValueChanged(e => maxHandledTapTouchesDecremented = e.NewValue - 1, true);
        }

        /// <summary>
        /// Handles how we behave when we don't accept cursor movement (e.g. autopilot).
        /// </summary>
        public void HandleAllowUserCursorMovement(bool allowUserCursorMovement)
        {
            if (!allowUserCursorMovement)
                maxHandledTapTouches.Value -= 1;
        }

        /// <summary>
        /// This is the touch assigned to the cursor.
        /// </summary>
        private TouchSource? cursorTouchSource;

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a tap touch.
        /// </summary>
        /// <param name="source">The <see cref="TouchSource"/> to check.</param>
        /// <returns>Whether the given source is a tap touch.</returns>
        public bool IsTapTouch(TouchSource source) => cursorTouchSource.HasValue && source != cursorTouchSource.Value;

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a cursor touch.
        /// </summary>
        /// <param name="source">The <see cref="TouchSource"/> to check.</param>
        /// <returns>Whether the given source is the cursor touch.</returns>
        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        /// <summary>
        /// Whether we can still handle another touch input.
        /// </summary>
        public bool AcceptingTouchInputs => ActiveTapTouchesCount <= maxHandledTapTouchesDecremented;

        /// <summary>
        /// Whether stream mode is enabled.
        /// When stream mode is enabled the cursor's handled action is disabled.
        /// This means all the tapping must be done by other fingers.
        /// </summary>
        public bool IsStreamMode;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            if (!AcceptingTouchInputs) return base.OnTouchDown(e);

            var source = e.Touch.Source;

            if (IsCursorTouch(source))
            {
                cursorTouchSource = source;

                return base.OnTouchDown(e);
            }

            var action = ActiveTapTouchesCount % 2 == 0 ? OsuAction.RightButton : OsuAction.LeftButton;

            triggeredActions.Add(source, action);

            if (osuInputManager.AllowUserCursorMovement && !IsStreamMode && ActiveTapTouchesCount == maxHandledTapTouches.Value)
            {
                IsStreamMode = true;
                osuInputManager.EnableStreamMode(cursorTouchSource!.Value);
            }

            keyBindingContainer.TriggerPressed(action);

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;

            if (triggeredActions.ContainsKey(source))
            {
                keyBindingContainer.TriggerReleased(triggeredActions[source]);
                triggeredActions.Remove(source);
            }
            else if (IsCursorTouch(source))
            {
                cursorTouchSource = null;
                IsStreamMode = false;
            }

            base.OnTouchUp(e);
        }
    }
}
