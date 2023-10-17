// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
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
            mousePosition = Parent!.ToLocalSpace(e.ScreenSpaceMousePosition);
            return base.OnMouseMove(e);
        }

        protected override void Update()
        {
            base.Update();

            if (mousePosition is Vector2 pos)
            {
                float thisAngle = -MathUtils.RadiansToDegrees(MathF.Atan2(pos.X - DrawSize.X / 2, pos.Y - DrawSize.Y / 2));
                float delta = lastAngle == null ? 0 : thisAngle - lastAngle.Value;

                // Normalise the delta to -180 .. 180
                if (delta > 180) delta -= 360;
                if (delta < -180) delta += 360;

                if (Tracking)
                    AddRotation(delta);

                lastAngle = thisAngle;
            }

            IsSpinning.Value = isSpinnableTime && Math.Abs(currentRotation - Rotation) > 10f;
            Rotation = (float)Interpolation.Damp(Rotation, currentRotation, 0.99, Math.Abs(Time.Elapsed));
        }

        /// <summary>
        /// Rotate the disc by the provided angle (in addition to any existing rotation).
        /// </summary>
        /// <remarks>
        /// Will be a no-op if not a valid time to spin.
        /// </remarks>
        /// <param name="delta">The delta angle.</param>
        public void AddRotation(float delta)
        {
            if (!isSpinnableTime)
                return;

            if (!rotationTransferred)
            {
                currentRotation = Rotation;
                rotationTransferred = true;
            }

            currentRotation += delta;

            double rate = gameplayClock?.GetTrueGameplayRate() ?? Clock.Rate;

            Debug.Assert(Math.Abs(delta) <= 180);

            // rate has to be applied each frame, because it's not guaranteed to be constant throughout playback
            // (see: ModTimeRamp)
            drawableSpinner.Result.TotalRotation += (float)(Math.Abs(delta) * rate);
        }

        private void resetState(DrawableHitObject obj)
        {
            Tracking = false;
            IsSpinning.Value = false;
            mousePosition = null;
            lastAngle = null;
            currentRotation = 0;
            Rotation = 0;
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
