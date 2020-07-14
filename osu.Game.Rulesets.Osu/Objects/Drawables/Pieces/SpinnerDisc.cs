// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerDisc : CircularContainer, IHasAccentColour
    {
        private readonly Spinner spinner;

        public Color4 AccentColour
        {
            get => background.AccentColour;
            set => background.AccentColour = value;
        }

        private readonly SpinnerBackground background;

        private const float idle_alpha = 0.2f;
        private const float tracking_alpha = 0.4f;

        public override bool IsPresent => true; // handle input when hidden

        public SpinnerDisc(Spinner s)
        {
            spinner = s;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                background = new SpinnerBackground { Alpha = idle_alpha },
            };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private bool tracking;

        public bool Tracking
        {
            get => tracking;
            set
            {
                if (value == tracking) return;

                tracking = value;

                background.FadeTo(tracking ? tracking_alpha : idle_alpha, 100);
            }
        }

        private bool complete;

        public bool Complete
        {
            get => complete;
            set
            {
                if (value == complete) return;

                complete = value;

                updateCompleteTick();
            }
        }

        /// <summary>
        /// The total rotation performed on the spinner disc, disregarding the spin direction.
        /// </summary>
        /// <remarks>
        /// This value is always non-negative and is monotonically increasing with time
        /// (i.e. will only increase if time is passing forward, but can decrease during rewind).
        /// </remarks>
        /// <example>
        /// If the spinner is spun 360 degrees clockwise and then 360 degrees counter-clockwise,
        /// this property will return the value of 720 (as opposed to 0 for <see cref="Drawable.Rotation"/>).
        /// </example>
        public float CumulativeRotation;

        /// <summary>
        /// Whether currently in the correct time range to allow spinning.
        /// </summary>
        private bool isSpinnableTime => spinner.StartTime <= Time.Current && spinner.EndTime > Time.Current;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            mousePosition = Parent.ToLocalSpace(e.ScreenSpaceMousePosition);
            return base.OnMouseMove(e);
        }

        private Vector2 mousePosition;

        private float lastAngle;
        private float currentRotation;
        private int completeTick;

        private bool updateCompleteTick() => completeTick != (completeTick = (int)(CumulativeRotation / 360));

        private bool rotationTransferred;

        protected override void Update()
        {
            base.Update();
            var thisAngle = -MathUtils.RadiansToDegrees(MathF.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));

            var delta = thisAngle - lastAngle;

            if (tracking)
                Rotate(delta);

            lastAngle = thisAngle;

            if (Complete && updateCompleteTick())
            {
                background.FinishTransforms(false, nameof(Alpha));
                background
                    .FadeTo(tracking_alpha + 0.2f, 60, Easing.OutExpo)
                    .Then()
                    .FadeTo(tracking_alpha, 250, Easing.OutQuint);
            }

            Rotation = (float)Interpolation.Lerp(Rotation, currentRotation / 2, Math.Clamp(Math.Abs(Time.Elapsed) / 40, 0, 1));
        }

        /// <summary>
        /// Rotate the disc by the provided angle (in addition to any existing rotation).
        /// </summary>
        /// <remarks>
        /// Will be a no-op if not a valid time to spin.
        /// </remarks>
        /// <param name="angle">The delta angle.</param>
        public void Rotate(float angle)
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
            CumulativeRotation += Math.Abs(angle) * Math.Sign(Clock.ElapsedFrameTime);
        }
    }
}
