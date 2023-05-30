// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
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

        [Resolved]
        private IGameplayClock? gameplayClock { get; set; }

        [Resolved]
        private IFrameStableClock? frameStableClock { get; set; }

        private Stack<(double time, float rotation)> rotationHistory = new Stack<(double time, float rotation)>();

        public SpinnerRotationTracker(DrawableSpinner drawableSpinner)
        {
            this.drawableSpinner = drawableSpinner;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (frameStableClock == null)
            {
                rotationHistory = new Stack<(double time, float rotation)>();
            }
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

            if (frameStableClock?.FrameStablePlayback != true)
            {
                // This class already guarantees correct rewind behaviour via the rate-adjusted calculations, but
                // this only holds as long as spinners are within a frame-stable playback environment.
                //
                // This is not the case in the editor, for instance, which results in rotation tracking not working as expected.
                // As a temporary measure, let's store the history of rotations and use it only in such cases.
                //
                // Reasoning for not always using this is because we are already quite confident with the existing methodology of this class,
                // and I'd rather not potentially break that in the process.
                var clock = (gameplayClock ?? Clock);

                while (rotationHistory.TryPeek(out (double time, float rotation) pair))
                {
                    if (pair.time < clock.CurrentTime)
                        break;

                    drawableSpinner.Result.RateAdjustedRotation = pair.rotation;
                    rotationHistory.Pop();
                }
            }

            float thisAngle = -MathUtils.RadiansToDegrees(MathF.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));

            float delta = thisAngle - lastAngle;

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
            drawableSpinner.Result.RateAdjustedRotation += (float)(Math.Abs(angle) * (gameplayClock?.GetTrueGameplayRate() ?? Clock.Rate));

            if (frameStableClock?.FrameStablePlayback != true)
            {
                rotationHistory.Push(((gameplayClock ?? Clock).CurrentTime, drawableSpinner.Result.RateAdjustedRotation));
            }
        }

        public void Reset()
        {
            Tracking = false;
            IsSpinning.Value = false;
            mousePosition = default;
            lastAngle = currentRotation = Rotation = 0;
            rotationTransferred = false;
            rotationHistory?.Clear();
        }
    }
}
