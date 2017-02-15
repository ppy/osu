// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.Logging;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerDisc : CircularContainer
    {
        public override bool Contains(Vector2 screenSpacePos) => true;

        protected Sprite Disc;

        public SRGBColour DiscColour
        {
            get { return Disc.Colour; }
            set { Disc.Colour = value; }
        }

        class SpinnerBorder : Container
        {
            public SpinnerBorder()
            {
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;

                layout();
            }

            private int lastLayoutDotCount;
            private void layout()
            {
                int count = (int)(MathHelper.Pi * ScreenSpaceDrawQuad.Width / 9);

                if (count == lastLayoutDotCount) return;

                lastLayoutDotCount = count;

                while (Children.Count() < count)
                {
                    Add(new CircularContainer
                    {
                        Colour = Color4.White,
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.Centre,
                        Size = new Vector2(1 / ScreenSpaceDrawQuad.Width * 2000),
                        Children = new[]
                        {
                            new Box
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
                    });
                }

                var size = new Vector2(1 / ScreenSpaceDrawQuad.Width * 2000);

                int i = 0;
                foreach (var d in Children)
                {
                    d.Size = size;
                    d.Position = new Vector2(
                        0.5f + (float)Math.Sin((float)i / count * 2 * MathHelper.Pi) / 2,
                        0.5f + (float)Math.Cos((float)i / count * 2 * MathHelper.Pi) / 2
                    );

                    i++;
                }
            }

            protected override void Update()
            {
                base.Update();
                layout();
            }
        }

        public SpinnerDisc()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                Disc = new Box
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f,
                },
                new SpinnerBorder()
            };
        }

        bool tracking;
        public bool Tracking
        {
            get { return tracking; }
            set
            {
                if (value == tracking) return;

                tracking = value;

                Disc.FadeTo(tracking ? 0.5f : 0.2f, 100);
            }
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Tracking = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Tracking = false;
            return base.OnMouseUp(state, args);
        }

        protected override bool OnMouseMove(InputState state)
        {
            Tracking |= state.Mouse.HasMainButtonPressed;
            mousePosition = state.Mouse.Position;
            return base.OnMouseMove(state);
        }

        private Vector2 mousePosition;

        private float lastAngle;
        private float currentRotation;
        public float RotationAbsolute;

        protected override void Update()
        {
            base.Update();

            var thisAngle = -(float)MathHelper.RadiansToDegrees(Math.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));
            if (tracking)
            {
                if (thisAngle - lastAngle > 180)
                    lastAngle += 360;
                else if (lastAngle - thisAngle > 180)
                    lastAngle -= 360;

                currentRotation += thisAngle - lastAngle;
                RotationAbsolute += Math.Abs(thisAngle - lastAngle);
            }
            lastAngle = thisAngle;

            RotateTo(currentRotation, 100, EasingTypes.OutExpo);
        }
    }
}
