// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuSliderBar<T> : SliderBar<T>, IHasTooltip where T : struct
    {
        private SampleChannel sample;
        private double lastSampleTime;
        private T lastSampleValue;

        private readonly Nub nub;
        private readonly Box leftBox;
        private readonly Box rightBox;

        public string TooltipText
        {
            get
            {
                var bindableDouble = CurrentNumber as BindableNumber<double>;
                if (bindableDouble != null)
                {
                    if (bindableDouble.MaxValue == 1 && bindableDouble.MinValue == 0)
                        return bindableDouble.Value.ToString(@"P0");
                    return bindableDouble.Value.ToString(@"n1");
                }

                var bindableInt = CurrentNumber as BindableNumber<int>;
                if (bindableInt != null)
                    return bindableInt.Value.ToString(@"n0");

                return Current.Value.ToString();
            }
        }

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
                    Expanded = true,
                }
            };

            Current.DisabledChanged += disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sample = audio.Sample.Get(@"Sliderbar/sliderbar");
            leftBox.Colour = colours.Pink;
            rightBox.Colour = colours.Pink;
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

        protected override void OnUserChange()
        {
            base.OnUserChange();
            playSample();
        }

        private void playSample()
        {
            if (Clock == null || Clock.CurrentTime - lastSampleTime <= 50)
                return;

            if (Current.Value.Equals(lastSampleValue))
                return;

            lastSampleValue = Current.Value;

            lastSampleTime = Clock.CurrentTime;
            sample.Frequency.Value = 1 + NormalizedValue * 0.2f;

            if (NormalizedValue == 0)
                sample.Frequency.Value -= 0.4f;
            else if (NormalizedValue == 1)
                sample.Frequency.Value += 0.4f;

            sample.Play();
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            nub.Current.Value = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            nub.Current.Value = false;
            return base.OnMouseUp(state, args);
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
