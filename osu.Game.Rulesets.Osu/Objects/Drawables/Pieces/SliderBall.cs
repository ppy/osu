// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Skinning;
using osuTK.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBall : CircularContainer, ISliderProgress, IRequireHighFrequencyMousePosition
    {
        public Func<OsuAction?> GetInitialHitAction;

        private readonly Slider slider;
        private readonly Drawable followCircle;
        private readonly DrawableSlider drawableSlider;

        public SliderBall(Slider slider, DrawableSlider drawableSlider = null)
        {
            this.drawableSlider = drawableSlider;
            this.slider = slider;

            Blending = BlendingParameters.Additive;
            Origin = Anchor.Centre;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Children = new[]
            {
                followCircle = new FollowCircleContainer
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Child = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderFollowCircle), _ => new DefaultFollowCircle()),
                },
                new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 1,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderBall), _ => new DefaultSliderBall()),
                    }
                }
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
            base.ApplyTransformsAt(time, false);
        }

        private bool tracking;

        public bool Tracking
        {
            get => tracking;
            private set
            {
                if (value == tracking)
                    return;

                tracking = value;

                followCircle.ScaleTo(tracking ? 2.4f : 1f, 300, Easing.OutQuint);
                followCircle.FadeTo(tracking ? 1f : 0, 300, Easing.OutQuint);
            }
        }

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

        protected override void Update()
        {
            base.Update();

            // from the point at which the head circle is hit, this will be non-null.
            // it may be null if the head circle was missed.
            var headCircleHitAction = GetInitialHitAction();

            if (headCircleHitAction == null)
                timeToAcceptAnyKeyAfter = null;

            var actions = drawableSlider?.OsuActionInputManager?.PressedActions;

            // if the head circle was hit with a specific key, tracking should only occur while that key is pressed.
            if (headCircleHitAction != null && timeToAcceptAnyKeyAfter == null)
            {
                var otherKey = headCircleHitAction == OsuAction.RightButton ? OsuAction.LeftButton : OsuAction.RightButton;

                // we can return to accepting all keys if the initial head circle key is the *only* key pressed, or all keys have been released.
                if (actions?.Contains(otherKey) != true)
                    timeToAcceptAnyKeyAfter = Time.Current;
            }

            Tracking =
                // in valid time range
                Time.Current >= slider.StartTime && Time.Current < slider.EndTime &&
                // in valid position range
                lastScreenSpaceMousePosition.HasValue && followCircle.ReceivePositionalInputAt(lastScreenSpaceMousePosition.Value) &&
                // valid action
                (actions?.Any(isValidTrackingAction) ?? false);
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
            var newPos = slider.CurvePositionAt(completionProgress);

            var diff = lastPosition.HasValue ? lastPosition.Value - newPos : newPos - slider.CurvePositionAt(completionProgress + 0.01f);
            if (diff == Vector2.Zero)
                return;

            Position = newPos;
            Rotation = -90 + (float)(-Math.Atan2(diff.X, diff.Y) * 180 / Math.PI);

            lastPosition = newPos;
        }

        private class FollowCircleContainer : Container
        {
            public override bool HandlePositionalInput => true;
        }

        public class DefaultFollowCircle : CompositeDrawable
        {
            public DefaultFollowCircle()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 5,
                    BorderColour = Color4.Orange,
                    Blending = BlendingParameters.Additive,
                    Child = new Box
                    {
                        Colour = Color4.Orange,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.2f,
                    }
                };
            }
        }

        public class DefaultSliderBall : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(DrawableHitObject drawableObject, ISkinSource skin)
            {
                RelativeSizeAxes = Axes.Both;

                float radius = skin.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.SliderPathRadius)?.Value ?? OsuHitObject.OBJECT_RADIUS;

                InternalChild = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(radius / OsuHitObject.OBJECT_RADIUS),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BorderThickness = 10,
                    BorderColour = Color4.White,
                    Alpha = 1,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Alpha = 0.4f,
                    }
                };
            }
        }
    }
}
