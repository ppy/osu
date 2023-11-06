// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuTouchInputMapper : Drawable
    {
        /// <summary>
        /// All the active <see cref="TouchSource"/>s and the <see cref="OsuAction"/> that it triggered (if any).
        /// Ordered from oldest to newest touch chronologically.
        /// </summary>
        private readonly List<TrackedTouch> trackedTouches = new List<TrackedTouch>();

        /// <summary>
        /// The distance (in local pixels) that a touch must move before being considered a permanent tracking touch.
        /// After this distance is covered, any extra touches on the screen will be considered as button inputs, unless
        /// a new touch directly interacts with a hit circle.
        /// </summary>
        private const float distance_before_position_tracking_lock_in = 100;

        private TrackedTouch? positionTrackingTouch;

        private readonly OsuInputManager osuInputManager;

        private Bindable<bool> tapsDisabled = null!;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            tapsDisabled = config.GetBindable<bool>(OsuSetting.GameplayDisableTaps);
        }

        // Required to handle touches outside of the playfield when screen scaling is enabled.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override void OnTouchMove(TouchMoveEvent e)
        {
            base.OnTouchMove(e);
            handleTouchMovement(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            OsuAction action = trackedTouches.Any(t => t.Action == OsuAction.LeftButton)
                ? OsuAction.RightButton
                : OsuAction.LeftButton;

            // Ignore any taps which trigger an action which is already handled. But track them for potential positional input in the future.
            bool shouldResultInAction = osuInputManager.AllowGameplayInputs && !tapsDisabled.Value && trackedTouches.All(t => t.Action != action);

            // If we can actually accept as an action, check whether this tap was on a circle's receptor.
            // This case gets special handling to allow for empty-space stream tapping.
            bool isDirectCircleTouch = osuInputManager.CheckScreenSpaceActionPressJudgeable(e.ScreenSpaceTouchDownPosition);

            var newTouch = new TrackedTouch(e.Touch.Source, shouldResultInAction ? action : null, isDirectCircleTouch);

            updatePositionTracking(newTouch);

            trackedTouches.Add(newTouch);

            // Important to update position before triggering the pressed action.
            handleTouchMovement(e);

            if (shouldResultInAction)
                osuInputManager.KeyBindingContainer.TriggerPressed(action);

            return true;
        }

        /// <summary>
        /// Given a new touch, update the positional tracking state and any related operations.
        /// </summary>
        private void updatePositionTracking(TrackedTouch newTouch)
        {
            // If the new touch directly interacted with a circle's receptor, it always becomes the current touch for positional tracking.
            if (newTouch.DirectTouch)
            {
                positionTrackingTouch = newTouch;
                return;
            }

            // Otherwise, we only want to use the new touch for position tracking if no other touch is tracking position yet..
            if (positionTrackingTouch == null)
            {
                positionTrackingTouch = newTouch;
                return;
            }

            // ..or if the current position tracking touch was not a direct touch (and didn't travel across the screen too far).
            if (!positionTrackingTouch.DirectTouch && positionTrackingTouch.DistanceTravelled < distance_before_position_tracking_lock_in)
            {
                positionTrackingTouch = newTouch;
                return;
            }

            // In the case the new touch was not used for position tracking, we should also check the previous position tracking touch.
            // If it still has its action pressed, that action should be released.
            //
            // This is done to allow tracking with the initial touch while still having both Left/Right actions available for alternating with two more touches.
            if (positionTrackingTouch.Action is OsuAction touchAction)
            {
                osuInputManager.KeyBindingContainer.TriggerReleased(touchAction);
                positionTrackingTouch.Action = null;
            }
        }

        private void handleTouchMovement(TouchEvent touchEvent)
        {
            if (touchEvent is TouchMoveEvent moveEvent)
            {
                var trackedTouch = trackedTouches.Single(t => t.Source == touchEvent.Touch.Source);
                trackedTouch.DistanceTravelled += moveEvent.Delta.Length;
            }

            // Movement should only be tracked for the most recent touch.
            if (touchEvent.Touch.Source != positionTrackingTouch?.Source)
                return;

            if (!osuInputManager.AllowUserCursorMovement)
                return;

            new MousePositionAbsoluteInput { Position = touchEvent.ScreenSpaceTouch.Position }.Apply(osuInputManager.CurrentState, osuInputManager);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var tracked = trackedTouches.Single(t => t.Source == e.Touch.Source);

            if (tracked.Action is OsuAction action)
                osuInputManager.KeyBindingContainer.TriggerReleased(action);

            if (positionTrackingTouch == tracked)
                positionTrackingTouch = null;

            trackedTouches.Remove(tracked);

            base.OnTouchUp(e);
        }

        private class TrackedTouch
        {
            public readonly TouchSource Source;

            public OsuAction? Action;

            /// <summary>
            /// Whether the touch was on a hit circle receptor.
            /// </summary>
            public readonly bool DirectTouch;

            /// <summary>
            /// The total distance on screen travelled by this touch (in local pixels).
            /// </summary>
            public float DistanceTravelled;

            public TrackedTouch(TouchSource source, OsuAction? action, bool directTouch)
            {
                Source = source;
                Action = action;
                DirectTouch = directTouch;
            }
        }
    }
}
