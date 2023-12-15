// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class SliderInputManager : Component, IRequireHighFrequencyMousePosition
    {
        /// <summary>
        /// Whether the slider is currently being tracked.
        /// </summary>
        public bool Tracking { get; private set; }

        /// <summary>
        /// The point in time after which we can accept any key for tracking. Before this time, we may need to restrict tracking to the key used to hit the head circle.
        ///
        /// This is a requirement to stop the case where a player holds down one key (from before the slider) and taps the second key while maintaining full scoring (tracking) of sliders.
        /// Visually, this special case can be seen below (time increasing from left to right):
        ///
        ///  Z  Z+X  Z
        ///      o========o
        ///
        /// Without this logic, tracking would continue through the entire slider even though no key hold action is directly attributing to it.
        ///
        /// In all other cases, no special handling is required (either key being pressed is allowable as valid tracking).
        ///
        /// The reason for storing this as a time value (rather than a bool) is to correctly handle rewind scenarios.
        /// </summary>
        private double? timeToAcceptAnyKeyAfter;

        /// <summary>
        /// The actions that were pressed in the previous frame.
        /// </summary>
        private readonly List<OsuAction> lastPressedActions = new List<OsuAction>();

        private Vector2? screenSpaceMousePosition;
        private readonly DrawableSlider slider;

        public SliderInputManager(DrawableSlider slider)
        {
            this.slider = slider;
        }

        /// <summary>
        /// This component handles all input of the slider, so it should receive input no matter the position.
        /// </summary>
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            screenSpaceMousePosition = e.ScreenSpaceMousePosition;
            return base.OnMouseMove(e);
        }

        protected override void Update()
        {
            base.Update();
            updateTracking(isMouseInFollowArea(Tracking));
        }

        public void PostProcessHeadJudgement(DrawableSliderHead head)
        {
            if (!head.Judged || !head.Result.IsHit)
                return;

            if (!isMouseInFollowArea(true))
                return;

            // When the head is hit and the mouse is in the expanded follow area, force a hit on every nested hitobject
            // from the start of the slider that is within follow-radius units from the head.

            bool forceMiss = false;

            foreach (var nested in slider.NestedHitObjects.OfType<DrawableOsuHitObject>())
            {
                // Skip nested objects that are already judged.
                if (nested.Judged)
                    continue;

                // Stop the process when a nested object is reached that can't be hit before the current time.
                if (nested.HitObject.StartTime > Time.Current)
                    break;

                float radius = getFollowRadius(true);
                double objectProgress = Math.Clamp((nested.HitObject.StartTime - slider.HitObject.StartTime) / slider.HitObject.Duration, 0, 1);
                Vector2 objectPosition = slider.HitObject.CurvePositionAt(objectProgress);

                // When the first nested object that is further than follow-radius units away from the start of the slider is reached,
                // forcefully miss all other nested objects that would otherwise be valid to be hit by this process.
                if (forceMiss || objectPosition.LengthSquared > radius * radius)
                {
                    nested.MissForcefully();
                    forceMiss = true;
                }
                else
                    nested.HitForcefully();
            }

            // Enable tracking, since the mouse is within the follow area (if it were expanded).
            updateTracking(true);
        }

        public void TryJudgeNestedObject(DrawableOsuHitObject nestedObject, double timeOffset)
        {
            switch (nestedObject)
            {
                case DrawableSliderRepeat:
                case DrawableSliderTick:
                    if (timeOffset < 0)
                        return;

                    break;

                case DrawableSliderTail:
                    if (timeOffset < SliderEventGenerator.TAIL_LENIENCY)
                        return;

                    // Ensure the tail can only activate after all previous ticks/repeats already have.
                    //
                    // This covers the edge case where the lenience may allow the tail to activate before
                    // the last tick, changing ordering of score/combo awarding.
                    var lastTick = slider.NestedHitObjects.LastOrDefault(o => o.HitObject is SliderTick || o.HitObject is SliderRepeat);
                    if (lastTick?.Judged == false)
                        return;

                    break;

                default:
                    return;
            }

            if (!slider.HeadCircle.Judged)
            {
                if (slider.Tracking.Value)
                {
                    // Attempt to preserve correct ordering of judgements as best we can by forcing an un-judged head to be missed when the user has clearly skipped it.
                    slider.HeadCircle.MissForcefully();
                }
                else
                {
                    // Don't judge this object as a miss before the head has been judged, to allow the head to be hit late.
                    return;
                }
            }

            if (slider.Tracking.Value)
                nestedObject.HitForcefully();
            else
                nestedObject.MissForcefully();
        }

        /// <summary>
        /// Whether the mouse is currently in the follow area.
        /// </summary>
        /// <param name="expanded">Whether to test against the maximum area of the follow circle.</param>
        private bool isMouseInFollowArea(bool expanded)
        {
            if (screenSpaceMousePosition is not Vector2 pos)
                return false;

            float radius = getFollowRadius(expanded);

            double followProgress = Math.Clamp((Time.Current - slider.HitObject.StartTime) / slider.HitObject.Duration, 0, 1);
            Vector2 followCirclePosition = slider.HitObject.CurvePositionAt(followProgress);
            Vector2 mousePositionInSlider = slider.ToLocalSpace(pos) - slider.OriginPosition;

            return (mousePositionInSlider - followCirclePosition).LengthSquared <= radius * radius;
        }

        /// <summary>
        /// Retrieves the radius of the follow area.
        /// </summary>
        /// <param name="expanded">Whether to return the maximum area of the follow circle.</param>
        private float getFollowRadius(bool expanded)
        {
            float radius = (float)slider.HitObject.Radius;

            if (expanded)
                radius *= DrawableSliderBall.FOLLOW_AREA;

            return radius;
        }

        /// <summary>
        /// Updates the tracking state.
        /// </summary>
        /// <param name="isValidTrackingPosition">Whether the current mouse position is valid to begin tracking.</param>
        private void updateTracking(bool isValidTrackingPosition)
        {
            // from the point at which the head circle is hit, this will be non-null.
            // it may be null if the head circle was missed.
            OsuAction? headCircleHitAction = getInitialHitAction();

            if (headCircleHitAction == null)
                timeToAcceptAnyKeyAfter = null;

            var actions = slider.OsuActionInputManager?.PressedActions;

            // if the head circle was hit with a specific key, tracking should only occur while that key is pressed.
            if (headCircleHitAction != null && timeToAcceptAnyKeyAfter == null)
            {
                var otherKey = headCircleHitAction == OsuAction.RightButton ? OsuAction.LeftButton : OsuAction.RightButton;

                // we can start accepting any key once all other keys have been released in the previous frame.
                if (!lastPressedActions.Contains(otherKey))
                    timeToAcceptAnyKeyAfter = Time.Current;
            }

            Tracking =
                // even in an edge case where current time has exceeded the slider's time, we may not have finished judging.
                // we don't want to potentially update from Tracking=true to Tracking=false at this point.
                (!slider.AllJudged || Time.Current <= slider.HitObject.GetEndTime())
                // in valid position range
                && isValidTrackingPosition
                // valid action
                && (actions?.Any(isValidTrackingAction) ?? false);

            lastPressedActions.Clear();
            if (actions != null)
                lastPressedActions.AddRange(actions);
        }

        private OsuAction? getInitialHitAction() => slider.HeadCircle?.HitAction;

        /// <summary>
        /// Check whether a given user input is a valid tracking action.
        /// </summary>
        private bool isValidTrackingAction(OsuAction action)
        {
            OsuAction? hitAction = getInitialHitAction();

            // if the head circle was hit, we may not yet be allowed to accept any key, so we must use the initial hit action.
            if (hitAction.HasValue && (!timeToAcceptAnyKeyAfter.HasValue || Time.Current <= timeToAcceptAnyKeyAfter.Value))
                return action == hitAction;

            return action == OsuAction.LeftButton || action == OsuAction.RightButton;
        }
    }
}
