using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderBall : CircularContainer, ISliderProgress, IRequireHighFrequencyMousePosition, IHasAccentColour
    {
        public Func<OsuAction?> GetInitialHitAction;

        public Color4 AccentColour
        {
            get => ball.Colour;
            set => ball.Colour = value;
        }

        /// <summary>
        /// Whether to track accurately to the visual size of this <see cref="DrawableSliderBall"/>.
        /// If <c>false</c>, tracking will be performed at the final scale at all times.
        /// </summary>
        public bool InputTracksVisualSize = true;

        private readonly DrawableSlider drawableSlider;
        private readonly SkinnableDrawable followCircle;
        private readonly SkinnableDrawable ball;

        private readonly FollowReceptor followArea;

        public DrawableSliderBall(DrawableSlider drawableSlider)
        {
            this.drawableSlider = drawableSlider;

            Origin = Anchor.Centre;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Children = new Drawable[]
            {
                followArea = new FollowReceptor
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                followCircle = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderFollowCircle), _ => new DefaultFollowCircle())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                ball = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderBall), _ => new DefaultSliderBall())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        private Vector2? lastScreenSpaceMousePosition;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            lastScreenSpaceMousePosition = e.ScreenSpaceMousePosition;
            return base.OnMouseMove(e);
        }

        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null)
        {
            // Consider the case of rewinding - children's transforms are handled internally, so propagating down
            // any further will cause weirdness with the Tracking bool below. Let's not propagate further at this point.
            base.ClearTransformsAfter(time, false, targetMember);
        }

        public override void ApplyTransformsAt(double time, bool propagateChildren = false)
        {
            // For the same reasons as above w.r.t rewinding, we shouldn't propagate to children here either.
            // ReSharper disable once RedundantArgumentDefaultValue - removing the "redundant" default value triggers BaseMethodCallWithDefaultParameter
            base.ApplyTransformsAt(time, false);
        }

        public bool Tracking { get; private set; }

        /// <summary>
        /// If the cursor moves out of the ball's radius we still need to be able to receive positional updates to stop tracking.
        /// </summary>
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

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

        protected override void Update()
        {
            base.Update();

            // from the point at which the head circle is hit, this will be non-null.
            // it may be null if the head circle was missed.
            var headCircleHitAction = GetInitialHitAction();

            if (headCircleHitAction == null)
                timeToAcceptAnyKeyAfter = null;

            var actions = drawableSlider.OsuActionInputManager?.PressedActions;

            // if the head circle was hit with a specific key, tracking should only occur while that key is pressed.
            if (headCircleHitAction != null && timeToAcceptAnyKeyAfter == null)
            {
                var otherKey = headCircleHitAction == OsuAction.RightButton ? OsuAction.LeftButton : OsuAction.RightButton;

                // we can start accepting any key once all other keys have been released in the previous frame.
                if (!lastPressedActions.Contains(otherKey))
                    timeToAcceptAnyKeyAfter = Time.Current;
            }

            Tracking =
                // in valid time range
                Time.Current >= drawableSlider.HitObject.StartTime && Time.Current < drawableSlider.HitObject.EndTime &&
                // in valid position range
                lastScreenSpaceMousePosition.HasValue && followArea.ReceivePositionalInputAt(lastScreenSpaceMousePosition.Value) &&
                // valid action
                (actions?.Any(isValidTrackingAction) ?? false);

            lastPressedActions.Clear();
            if (actions != null)
                lastPressedActions.AddRange(actions);
        }

        /// <summary>
        /// Check whether a given user input is a valid tracking action.
        /// </summary>
        private bool isValidTrackingAction(OsuAction action)
        {
            bool headCircleHit = GetInitialHitAction().HasValue;

            // if the head circle was hit, we may not yet be allowed to accept any key, so we must use the initial hit action.
            if (headCircleHit && (!timeToAcceptAnyKeyAfter.HasValue || Time.Current <= timeToAcceptAnyKeyAfter.Value))
                return action == GetInitialHitAction();

            return action == OsuAction.LeftButton || action == OsuAction.RightButton;
        }

        private Vector2? lastPosition;

        public void UpdateProgress(double completionProgress)
        {
            var newPos = drawableSlider.HitObject.CurvePositionAt(completionProgress);

            var diff = lastPosition.HasValue ? lastPosition.Value - newPos : newPos - drawableSlider.HitObject.CurvePositionAt(completionProgress + 0.01f);
            if (diff == Vector2.Zero)
                return;

            Position = newPos;
            ball.Rotation = -90 + (float)(-Math.Atan2(diff.X, diff.Y) * 180 / Math.PI);

            lastPosition = newPos;
        }

        private class FollowReceptor : CircularContainer
        {
            public override bool HandlePositionalInput => true;

            private DrawableSliderBall sliderBall;

            [BackgroundDependencyLoader]
            private void load(DrawableHitObject drawableObject)
            {
                var slider = (DrawableSlider)drawableObject;
                sliderBall = slider.Ball;

                RelativeSizeAxes = Axes.Both;

                slider.Tracking.BindValueChanged(trackingChanged, true);
            }

            private void trackingChanged(ValueChangedEvent<bool> e)
            {
                bool tracking = e.NewValue;

                if (sliderBall.InputTracksVisualSize)
                    this.ScaleTo(tracking ? 2.4f : 1f, 300, Easing.OutQuint);
                else
                {
                    // We need to always be tracking the final size, at both endpoints. For now, this is achieved by removing the scale duration.
                    this.ScaleTo(tracking ? 2.4f : 1f);
                }
            }
        }
    }
}
