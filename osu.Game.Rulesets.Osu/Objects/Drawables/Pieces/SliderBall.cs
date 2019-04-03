// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBall : CircularContainer, ISliderProgress, IRequireHighFrequencyMousePosition
    {
        private const float width = 128;

        private Color4 accentColour = Color4.Black;

        public Func<OsuAction?> GetInitialHitAction;

        /// <summary>
        /// The colour that is used for the slider ball.
        /// </summary>
        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                if (drawableBall != null)
                    drawableBall.Colour = value;
            }
        }

        private readonly Slider slider;
        public readonly Drawable FollowCircle;
        private Drawable drawableBall;
        private readonly DrawableSlider drawableSlider;

        public SliderBall(Slider slider, DrawableSlider drawableSlider = null)
        {
            this.drawableSlider = drawableSlider;
            this.slider = slider;
            Masking = true;
            AutoSizeAxes = Axes.Both;
            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new[]
            {
                FollowCircle = new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Width = width,
                    Height = width,
                    Alpha = 0,
                    Child = new SkinnableDrawable("Play/osu/sliderfollowcircle", _ => new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = 5,
                        BorderColour = Color4.Orange,
                        Blending = BlendingMode.Additive,
                        Child = new Box
                        {
                            Colour = Color4.Orange,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.2f,
                        }
                    }),
                },
                new CircularContainer
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 1,
                    Child = new Container
                    {
                        Width = width,
                        Height = width,
                        // TODO: support skin filename animation (sliderb0, sliderb1...)
                        Child = new SkinnableDrawable("Play/osu/sliderb", _ => new CircularContainer
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 10,
                            BorderColour = Color4.White,
                            Alpha = 1,
                            Child = drawableBall = new Box
                            {
                                Colour = AccentColour,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.4f,
                            }
                        }),
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

                FollowCircle.ScaleTo(tracking ? 2f : 1, 300, Easing.OutQuint);
                FollowCircle.FadeTo(tracking ? 1f : 0, 300, Easing.OutQuint);
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
                lastScreenSpaceMousePosition.HasValue && base.ReceivePositionalInputAt(lastScreenSpaceMousePosition.Value) &&
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

        public void UpdateProgress(double completionProgress)
        {
            Position = slider.CurvePositionAt(completionProgress);
        }
    }
}
