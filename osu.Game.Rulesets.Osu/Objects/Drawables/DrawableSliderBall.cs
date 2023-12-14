// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSliderBall : CircularContainer, ISliderProgress, IRequireHighFrequencyMousePosition
    {
        public const float FOLLOW_AREA = 2.4f;

        public Func<OsuAction?> GetInitialHitAction;

        private Drawable followCircleReceptor;
        private DrawableSlider drawableSlider;
        private Drawable ball;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableSlider)
        {
            this.drawableSlider = (DrawableSlider)drawableSlider;

            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            Children = new[]
            {
                new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderFollowCircle), _ => new DefaultFollowCircle())
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                followCircleReceptor = new CircularContainer
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true
                },
                ball = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderBall), _ => new DefaultSliderBall())
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

            // ReSharper disable once RedundantArgumentDefaultValue
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

                followCircleReceptor.Scale = new Vector2(tracking ? FOLLOW_AREA : 1f);
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
                // even in an edge case where current time has exceeded the slider's time, we may not have finished judging.
                // we don't want to potentially update from Tracking=true to Tracking=false at this point.
                (!drawableSlider.AllJudged || Time.Current <= drawableSlider.HitObject.GetEndTime())
                // in valid position range
                && lastScreenSpaceMousePosition.HasValue && followCircleReceptor.ReceivePositionalInputAt(lastScreenSpaceMousePosition.Value) &&
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
            Position = drawableSlider.HitObject.CurvePositionAt(completionProgress);

            var diff = lastPosition.HasValue ? lastPosition.Value - Position : Position - drawableSlider.HitObject.CurvePositionAt(completionProgress + 0.01f);

            bool rewinding = (Clock as IGameplayClock)?.IsRewinding == true;

            // Ensure the value is substantially high enough to allow for Atan2 to get a valid angle.
            if (diff.LengthFast < 0.01f)
                return;

            ball.Rotation = -90 + (float)(-Math.Atan2(diff.X, diff.Y) * 180 / Math.PI) + (rewinding ? 180 : 0);
            lastPosition = Position;
        }
    }
}
