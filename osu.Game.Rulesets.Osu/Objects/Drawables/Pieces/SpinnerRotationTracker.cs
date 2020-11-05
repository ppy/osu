// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerRotationTracker : CircularContainer
    {
        public override bool IsPresent => true; // handle input when hidden

        private readonly DrawableSpinner drawableSpinner;

        public SpinnerRotationTracker(DrawableSpinner drawableSpinner)
        {
            this.drawableSpinner = drawableSpinner;

            RelativeSizeAxes = Axes.Both;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public bool Tracking { get; set; }

        public readonly BindableBool Complete = new BindableBool();

        /// <summary>
        /// The total rotation performed on the spinner disc, disregarding the spin direction,
        /// adjusted for the track's playback rate.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is always non-negative and is monotonically increasing with time
        /// (i.e. will only increase if time is passing forward, but can decrease during rewind).
        /// </para>
        /// <para>
        /// The rotation from each frame is multiplied by the clock's current playback rate.
        /// The reason this is done is to ensure that spinners give the same score and require the same number of spins
        /// regardless of whether speed-modifying mods are applied.
        /// </para>
        /// </remarks>
        /// <example>
        /// Assuming no speed-modifying mods are active,
        /// if the spinner is spun 360 degrees clockwise and then 360 degrees counter-clockwise,
        /// this property will return the value of 720 (as opposed to 0 for <see cref="Drawable.Rotation"/>).
        /// If Double Time is active instead (with a speed multiplier of 1.5x),
        /// in the same scenario the property will return 720 * 1.5 = 1080.
        /// </example>
        public float RateAdjustedRotation { get; private set; }

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
            RateAdjustedRotation += (float)(Math.Abs(angle) * (gameplayClock?.TrueGameplayRate ?? Clock.Rate));
        }
    }
}
