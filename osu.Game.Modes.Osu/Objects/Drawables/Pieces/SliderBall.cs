// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SliderBall : CircularContainer, ISliderProgress
    {
        private readonly Slider slider;
        private Box follow;

        const float width = 128;

        public SliderBall(Slider slider)
        {
            this.slider = slider;
            Masking = true;
            AutoSizeAxes = Axes.Both;
            BlendingMode = BlendingMode.Additive;
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
                        new Box
                        {
                            Colour = slider.Colour,
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

        bool tracking;
        public bool Tracking
        {
            get { return tracking; }
            set
            {
                if (value == tracking) return;

                tracking = value;

                follow.ScaleTo(tracking ? 2.8f : 1, 300, EasingTypes.OutQuint);
                follow.FadeTo(tracking ? 0.2f : 0, 300, EasingTypes.OutQuint);
            }
        }

        private bool canCurrentlyTrack => Time.Current >= slider.StartTime && Time.Current < slider.EndTime;

        protected override void Update()
        {
            base.Update();

            if (Time.Current < slider.EndTime)
                Tracking = canCurrentlyTrack && lastState != null && Contains(lastState.Mouse.NativeState.Position) && lastState.Mouse.HasMainButtonPressed;
        }

        public void UpdateProgress(double progress, int repeat)
        {
            Position = slider.Curve.PositionAt(progress);
        }
    }
}