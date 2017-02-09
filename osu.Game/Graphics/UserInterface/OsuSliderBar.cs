// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuSliderBar<U> : SliderBar<U> where U : struct
    {
        private AudioSample sample;
        private double lastSampleTime;

        private Nub nub;
        private Box leftBox, rightBox;

        public OsuSliderBar()
        {
            Height = 12;
            RangePadding = 20;
            Children = new Drawable[]
            {
                leftBox = new Box
                {
                    Height = 2,
                    EdgeSmoothness = new Vector2(0, 0.5f),
                    Position = new Vector2(2, 0),
                    RelativeSizeAxes = Axes.None,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                rightBox = new Box
                {
                    Height = 2,
                    EdgeSmoothness = new Vector2(0, 0.5f),
                    Position = new Vector2(-2, 0),
                    RelativeSizeAxes = Axes.None,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Alpha = 0.5f,
                },
                nub = new Nub
                {
                    Origin = Anchor.TopCentre,
                    State = CheckBoxState.Unchecked,
                    Expanded = true,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sample = audio.Sample.Get(@"Sliderbar/sliderbar");
            leftBox.Colour = colours.Pink;
            rightBox.Colour = colours.Pink;
        }

        private void playSample()
        {
            if (Clock == null || Clock.CurrentTime - lastSampleTime <= 50)
                return;
            lastSampleTime = Clock.CurrentTime;
            sample.Frequency.Value = 1 + NormalizedValue * 0.2f;
            sample.Play();
        }

        protected override bool OnHover(InputState state)
        {
            nub.Glowing = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            nub.Glowing = false;
            base.OnHoverLost(state);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Left || args.Key == Key.Right)
                playSample();
            return base.OnKeyDown(state, args);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            nub.State = CheckBoxState.Checked;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            nub.State = CheckBoxState.Unchecked;
            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            playSample();
            return base.OnClick(state);
        }

        protected override bool OnDrag(InputState state)
        {
            playSample();
            return base.OnDrag(state);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            leftBox.Scale = new Vector2(MathHelper.Clamp(
                nub.DrawPosition.X - nub.DrawWidth / 2, 0, DrawWidth), 1);
            rightBox.Scale = new Vector2(MathHelper.Clamp(
                DrawWidth - nub.DrawPosition.X - nub.DrawWidth / 2, 0, DrawWidth), 1);
        }

        protected override void UpdateValue(float value)
        {
            nub.MoveToX(RangePadding + UsableWidth * value, 250, EasingTypes.OutQuint);
        }
    }
}
