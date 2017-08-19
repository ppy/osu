﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerDisc : CircularContainer, IHasAccentColour
    {
        private readonly Spinner spinner;

        public Color4 AccentColour
        {
            get { return background.AccentColour; }
            set { background.AccentColour = value; }
        }

        private readonly SpinnerBackground background;

        private const float idle_alpha = 0.2f;
        private const float tracking_alpha = 0.4f;

        public SpinnerDisc(Spinner s)
        {
            spinner = s;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                background = new SpinnerBackground { Alpha = idle_alpha },
            };
        }

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        private bool tracking;
        public bool Tracking
        {
            get { return tracking; }
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
            get { return complete; }
            set
            {
                if (value == complete) return;
                complete = value;

                updateCompleteTick();
            }
        }

        protected override bool OnMouseMove(InputState state)
        {
            mousePosition = Parent.ToLocalSpace(state.Mouse.NativeState.Position);
            return base.OnMouseMove(state);
        }

        private Vector2 mousePosition;

        private float lastAngle;
        private float currentRotation;
        public float RotationAbsolute;

        private int completeTick;

        private bool updateCompleteTick() => completeTick != (completeTick = (int)(RotationAbsolute / 360));

        private bool rotationTransferred;

        protected override void Update()
        {
            base.Update();

            var thisAngle = -(float)MathHelper.RadiansToDegrees(Math.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));

            bool validAndTracking = tracking && spinner.StartTime <= Time.Current && spinner.EndTime > Time.Current;

            if (validAndTracking)
            {
                if (!rotationTransferred)
                {
                    currentRotation = Rotation * 2;
                    rotationTransferred = true;
                }

                if (thisAngle - lastAngle > 180)
                    lastAngle += 360;
                else if (lastAngle - thisAngle > 180)
                    lastAngle -= 360;

                currentRotation += thisAngle - lastAngle;
                RotationAbsolute += Math.Abs(thisAngle - lastAngle);
            }

            lastAngle = thisAngle;

            if (Complete && updateCompleteTick())
            {
                background.FinishTransforms(false, nameof(Alpha));
                background
                    .FadeTo(tracking_alpha + 0.2f, 60, Easing.OutExpo)
                    .Then()
                    .FadeTo(tracking_alpha, 250, Easing.OutQuint);
            }

            this.RotateTo(currentRotation / 2, validAndTracking ? 500 : 1500, Easing.OutExpo);
        }
    }
}
