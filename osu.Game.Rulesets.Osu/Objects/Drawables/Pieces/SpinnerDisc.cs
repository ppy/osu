// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerDisc : CircularContainer
    {
        protected Sprite Disc;

        public SRGBColour DiscColour
        {
            get { return Disc.Colour; }
            set { Disc.Colour = value; }
        }

        private Color4 completeColour;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            completeColour = colours.YellowLight.Opacity(0.8f);
            Masking = true;
        }

        private class SpinnerBorder : Container
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
                        Masking = true,
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
            AlwaysReceiveInput = true;

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

        private bool tracking;
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

        private bool complete;
        public bool Complete
        {
            get { return complete; }
            set
            {
                if (value == complete) return;

                complete = value;

                Disc.FadeColour(completeColour, 200);

                updateCompleteTick();
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

        private int completeTick;

        private bool updateCompleteTick() => completeTick != (completeTick = (int)(RotationAbsolute / 360));

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

            if (Complete && updateCompleteTick())
            {
                Disc.Flush(flushType: typeof(TransformAlpha));
                Disc.FadeTo(0.75f, 30, EasingTypes.OutExpo);
                Disc.Delay(30);
                Disc.FadeTo(0.5f, 250, EasingTypes.OutQuint);
            }

            RotateTo(currentRotation, 100, EasingTypes.OutExpo);
        }
    }
}
