// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
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

        private Vector2 mousePosition;

        private float lastAngle;
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
            float thisAngle = -MathUtils.RadiansToDegrees(MathF.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));

            float delta = thisAngle - lastAngle;

            if (Tracking)
                AddRotation(delta);

            lastAngle = thisAngle;

            IsSpinning.Value = isSpinnableTime && Math.Abs(currentRotation - Rotation) > 10f;

            Rotation = (float)Interpolation.Damp(Rotation, currentRotation, 0.99, Math.Abs(Time.Elapsed));
        }

        private float currentDelta;
        private int currentDirection;
        private readonly List<(double time, float delta)> turningPoints = new List<(double, float)>();

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
            double time = gameplayClock?.CurrentTime ?? Clock.CurrentTime;
            double rate = gameplayClock?.GetTrueGameplayRate() ?? Clock.Rate;
            float rateAdjustedRotation = (float)(angle * rate);

            if (rate > 0)
            {
                int direction = Math.Sign(rateAdjustedRotation);

                if (direction != 0 && direction != currentDirection)
                {
                    turningPoints.Add((time, currentDelta));
                    currentDelta = 0;
                }

                currentDirection = direction;
            }
            else if (turningPoints.Count > 0 && time <= turningPoints[^1].time)
            {
                currentDelta = turningPoints[^1].delta;
                currentDirection = Math.Sign(currentDelta);
                turningPoints.RemoveAt(turningPoints.Count - 1);
            }

            currentDelta += rateAdjustedRotation;
            drawableSpinner.Result.RateAdjustedRotation = computeEffectiveRotation();

            Logger.Log(currentDelta.ToString(CultureInfo.InvariantCulture));
        }

        private float computeEffectiveRotation()
        {
            float curAngle = 0;
            float maxAngle = 0;
            float minAngle = 0;
            float effectiveRotation = 0;

            for (int i = 0; i < turningPoints.Count; i++)
                applyDelta(turningPoints[i].delta);
            applyDelta(currentDelta);

            return effectiveRotation;

            void applyDelta(float delta)
            {
                float totalBefore = Math.Abs(maxAngle - minAngle);

                curAngle += delta;
                maxAngle = Math.Max(maxAngle, curAngle);
                minAngle = Math.Min(minAngle, curAngle);

                float totalAfter = Math.Abs(maxAngle - minAngle);

                effectiveRotation += totalAfter - totalBefore;

                // Check if we've started a new rotation.
                if (totalAfter < 360)
                    return;

                // Start a new rotation.
                // It is fine for totalAfter to skip one rotation (i.e be >= 720), since full rotations are properly accounted for in effectiveRotation.
                // All that matters is any partial rotation.
                float excess = (totalAfter % 360) * Math.Sign(delta);

                curAngle = excess;
                maxAngle = Math.Max(0, curAngle);
                minAngle = Math.Min(0, curAngle);
            }
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

            if (drawableSpinner.IsNotNull())
                drawableSpinner.HitObjectApplied -= resetState;
        }
    }
}
