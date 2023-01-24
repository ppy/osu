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

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuTouchInputMapper : Drawable
    {
        /// <summary>
        /// All the active <see cref="TouchSource"/>s and the <see cref="OsuAction"/> that it triggered (if any).
        /// Ordered from oldest to newest touch chronologically.
        /// </summary>
        private readonly List<TrackedTouch> trackedTouches = new List<TrackedTouch>();

        private TrackedTouch? positionTrackingTouch;

        private readonly OsuInputManager osuInputManager;

        private Bindable<bool> mouseDisabled = null!;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            // The mouse button disable setting affects touch. It's a bit weird.
            // This is mostly just doing the same as what is done in RulesetInputManager to match behaviour.
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
        }

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
            bool shouldResultInAction = osuInputManager.AllowGameplayInputs && !mouseDisabled.Value && trackedTouches.All(t => t.Action != action);

            // If we can actually accept as an action, check whether this tap was on a circle's receptor.
            // This case gets special handling to allow for empty-space stream tapping.
            bool isDirectCircleTouch = osuInputManager.CheckScreenSpaceActionPressJudgeable(e.ScreenSpaceTouchDownPosition);

            var trackedTouch = new TrackedTouch(e.Touch.Source, shouldResultInAction ? action : null, isDirectCircleTouch);

            if (isDirectCircleTouch)
                positionTrackingTouch = trackedTouch;
            else
            {
                // If no direct touch is registered, we always use the new (latest) touch for positional tracking.
                if (positionTrackingTouch?.DirectTouch != true)
                    positionTrackingTouch = trackedTouch;
                else
                {
                    // If not a direct circle touch, consider whether to release the an original direct touch.
                    if (positionTrackingTouch.DirectTouch && positionTrackingTouch.Action is OsuAction directTouchAction)
                    {
                        osuInputManager.KeyBindingContainer.TriggerReleased(directTouchAction);
                        positionTrackingTouch.Action = null;
                    }
                }
            }

            trackedTouches.Add(trackedTouch);

            // Important to update position before triggering the pressed action.
            handleTouchMovement(e);

            if (shouldResultInAction)
                osuInputManager.KeyBindingContainer.TriggerPressed(action);

            return true;
        }

        private void handleTouchMovement(TouchEvent touchEvent)
        {
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

            public readonly bool DirectTouch;

            public TrackedTouch(TouchSource source, OsuAction? action, bool directTouch)
            {
                Source = source;
                Action = action;
                DirectTouch = directTouch;
            }
        }
    }
}
