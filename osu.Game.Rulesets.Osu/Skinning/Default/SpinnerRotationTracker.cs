// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class SpinnerRotationTracker : CircularContainer
    {
        public override bool IsPresent => true; // handle input when hidden

        private readonly DrawableSpinner drawableSpinner;

        private Vector2? mousePosition;
        private float? lastAngle;

        private float currentRotation;
        private bool rotationTransferred;

        [Resolved(canBeNull: true)]
        private IGameplayClock? gameplayClock { get; set; }

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

        protected override void Update()
        {
            base.Update();

            if (mousePosition is Vector2 pos)
            {
                float thisAngle = -MathUtils.RadiansToDegrees(MathF.Atan2(pos.X - DrawSize.X / 2, pos.Y - DrawSize.Y / 2));
                float delta = lastAngle == null ? 0 : thisAngle - lastAngle.Value;

                if (Tracking)
                    AddRotation(delta);

                lastAngle = thisAngle;
            }

            IsSpinning.Value = isSpinnableTime && Math.Abs(currentRotation - Rotation) > 10f;
            Rotation = (float)Interpolation.Damp(Rotation, currentRotation, 0.99, Math.Abs(Time.Elapsed));
        }

        /// <summary>
        /// The total amount spun in the current turn.
        /// This ranges between -360 to +360.
        /// </summary>
        private float currentTurnTotal;

        /// <summary>
        /// The absolute value of <see cref="currentTurnTotal"/> that has been transferred to the judgement in the current turn.
        /// </summary>
        private float currentTurnTransferred;

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

            currentTurnTotal += (float)(angle * (gameplayClock?.GetTrueGameplayRate() ?? Clock.Rate));
            drawableSpinner.Result.RateAdjustedRotation += Math.Max(0, Math.Abs(currentTurnTotal) - currentTurnTransferred);

            if (currentTurnTotal <= -360)
            {
                currentTurnTotal += 360;
                currentTurnTransferred = 0;
            }
            else if (currentTurnTotal >= 360)
            {
                currentTurnTotal -= 360;
                currentTurnTransferred = 0;
            }

            currentTurnTransferred = Math.Max(currentTurnTransferred, Math.Abs(currentTurnTotal));
        }

        private void resetState(DrawableHitObject obj)
        {
            Tracking = default;
            IsSpinning.Value = default;
            mousePosition = default;
            lastAngle = default;
            currentRotation = default;
            Rotation = default;
            rotationTransferred = default;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner.IsNotNull())
                drawableSpinner.HitObjectApplied -= resetState;
        }
    }
}
