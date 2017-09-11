// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBall : CircularContainer, ISliderProgress
    {
        private const float width = 128;

        private Color4 accentColour = Color4.Black;
        /// <summary>
        /// The colour that is used for the slider ball.
        /// </summary>
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;
                if (ball != null)
                    ball.Colour = value;
            }
        }

        private readonly Slider slider;
        private readonly Box follow;
        private readonly Box ball;

        public SliderBall(Slider slider)
        {
            this.slider = slider;
            Masking = true;
            AutoSizeAxes = Axes.Both;
            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;
            BorderThickness = 10;
            BorderColour = Color4.Orange;

            Children = new Drawable[]
            {
                follow = new Box
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = Color4.Orange,
                    Width = width,
                    Height = width,
                    Alpha = 0,
                },
                new CircularContainer
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = 10,
                    BorderColour = Color4.White,
                    Alpha = 1,
                    Children = new[]
                    {
                        ball = new Box
                        {
                            Colour = AccentColour,
                            Alpha = 0.4f,
                            Width = width,
                            Height = width,
                        },
                    }
                }
            };
        }

        private InputState lastState;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            lastState = state;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            lastState = state;
            return base.OnMouseUp(state, args);
        }

        protected override bool OnMouseMove(InputState state)
        {
            lastState = state;
            return base.OnMouseMove(state);
        }

        // If the current time is between the start and end of the slider, we should track mouse input regardless of the cursor position.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => canCurrentlyTrack || base.ReceiveMouseInputAt(screenSpacePos);

        private bool tracking;
        public bool Tracking
        {
            get { return tracking; }
            private set
            {
                if (value == tracking) return;

                tracking = value;

                follow.ScaleTo(tracking ? 2.8f : 1, 300, Easing.OutQuint);
                follow.FadeTo(tracking ? 0.2f : 0, 300, Easing.OutQuint);
            }
        }

        private bool canCurrentlyTrack => Time.Current >= slider.StartTime && Time.Current < slider.EndTime;

        protected override void Update()
        {
            base.Update();

            // Make sure to use the base version of ReceiveMouseInputAt so that we correctly check the position.
            if (Time.Current < slider.EndTime)
                Tracking = canCurrentlyTrack && lastState != null && base.ReceiveMouseInputAt(lastState.Mouse.NativeState.Position) && ((Parent as DrawableSlider)?.OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false);
        }

        public void UpdateProgress(double progress, int repeat)
        {
            Position = slider.Curve.PositionAt(progress);
        }
    }
}