// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class SpinnerRotationTracker : CircularContainer
    {
        public override bool IsPresent => true; // handle input when hidden

        private readonly DrawableSpinner drawableSpinner;

        public SpinnerRotationTracker(DrawableSpinner drawableSpinner)
        {
            this.drawableSpinner = drawableSpinner;
            drawableSpinner.HitObjectApplied += resetState;

            RelativeSizeAxes = Axes.Both;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public bool Tracking { get; set; }

        /// <summary>
        /// Whether the spinning is spinning at a reasonable speed to be considered visually spinning.
        /// </summary>
        public readonly BindableBool IsSpinning = new BindableBool();

        /// <summary>
        /// Whether currently in the correct time range to allow spinning.
        /// </summary>
        private bool isSpinnableTime => drawableSpinner.HitObject.StartTime <= Time.Current && drawableSpinner.HitObject.EndTime > Time.Current;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            mousePosition = Parent.ToLocalSpace(e.ScreenSpaceMousePosition);
            return base.OnMouseMove(e);
        }

        private Vector2 mousePosition;

        private float lastAngle;
        private float currentRotation;

        private bool rotationTransferred;

        [Resolved(canBeNull: true)]
        private GameplayClock gameplayClock { get; set; }

        protected override void Update()
        {
            base.Update();
            var thisAngle = -MathUtils.RadiansToDegrees(MathF.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));

            var delta = thisAngle - lastAngle;

            if (Tracking)
                AddRotation(delta);

            lastAngle = thisAngle;

            IsSpinning.Value = isSpinnableTime && Math.Abs(currentRotation / 2 - Rotation) > 5f;

            Rotation = (float)Interpolation.Damp(Rotation, currentRotation / 2, 0.99, Math.Abs(Time.Elapsed));
        }

        /// <summary>
        /// Rotate the disc by the provided angle (in addition to any existing rotation).
        /// </summary>
        /// <remarks>
        /// Will be a no-op if not a valid time to spin.
        /// </remarks>
        /// <param name="angle">The delta angle.</param>
        public void AddRotation(float angle)
        {
            if (!isSpinnableTime)
                return;

            if (!rotationTransferred)
            {
                currentRotation = Rotation * 2;
                rotationTransferred = true;
            }

            if (angle > 180)
            {
                lastAngle += 360;
                angle -= 360;
            }
            else if (-angle > 180)
            {
                lastAngle -= 360;
                angle += 360;
            }

            currentRotation += angle;
            // rate has to be applied each frame, because it's not guaranteed to be constant throughout playback
            // (see: ModTimeRamp)
            drawableSpinner.Result.RateAdjustedRotation += (float)(Math.Abs(angle) * (gameplayClock?.TrueGameplayRate ?? Clock.Rate));
        }

        private void resetState(DrawableHitObject obj)
        {
            Tracking = false;
            IsSpinning.Value = false;
            mousePosition = default;
            lastAngle = currentRotation = Rotation = 0;
            rotationTransferred = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner != null)
                drawableSpinner.HitObjectApplied -= resetState;
        }
    }
}
