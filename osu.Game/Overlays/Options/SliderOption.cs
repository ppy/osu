//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using System.Linq;

namespace osu.Game.Overlays.Options
{
    public class SliderOption<T> : FlowContainer where T : struct
    {
        private SliderBar<T> slider;
        private SpriteText text;
    
        public string LabelText
        {
            get { return text.Text; }
            set
            {
                text.Text = value;
                text.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
            }
        }
        
        public BindableNumber<T> Bindable
        {
            get { return slider.Bindable; }
            set { slider.Bindable = value; }
        }

        public SliderOption()
        {
            Direction = FlowDirection.VerticalOnly;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new SpriteText { Alpha = 0 },
                slider = new OsuSliderBar<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                }
            };
        }

        private class OsuSliderBar<U> : SliderBar<U> where U : struct
        {
            private AudioSample sample;
            private double lastSampleTime;
            
            private Container nub;
            private Box leftBox, rightBox;
            
            private float innerWidth
            {
                get
                {
                    return DrawWidth - Height;
                }
            }
            
            public OsuSliderBar()
            {
                Height = 22;
                Padding = new MarginPadding { Left = Height / 2, Right = Height / 2 };
                Children = new Drawable[]
                {
                    leftBox = new Box
                    {
                        Height = 2,
                        RelativeSizeAxes = Axes.None,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    rightBox = new Box
                    {
                        Height = 2,
                        RelativeSizeAxes = Axes.None,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Alpha = 0.5f,
                    },
                    nub = new Container
                    {
                        Width = Height,
                        Height = Height,
                        CornerRadius = Height / 2,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.None,
                        RelativeSizeAxes = Axes.None,
                        Masking = true,
                        BorderThickness = 3,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio, OsuColour colours)
            {
                sample = audio.Sample.Get(@"Sliderbar/sliderbar");
                leftBox.Colour = colours.Pink;
                rightBox.Colour = colours.Pink;
                nub.BorderColour = colours.Pink;
                (nub.Children.First() as Box).Colour = colours.Pink.Opacity(0);
            }

            private void playSample()
            {
                if (Clock == null || Clock.CurrentTime - lastSampleTime <= 50)
                    return;
                lastSampleTime = Clock.CurrentTime;
                sample.Frequency.Value = 1 + NormalizedValue * 0.2f;
                sample.Play();
            }
            
            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (args.Key == Key.Left || args.Key == Key.Right)
                    playSample();
                return base.OnKeyDown(state, args);
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
            
            protected override void Update()
            {
                base.Update();
                leftBox.Scale = new Vector2(MathHelper.Clamp(
                    nub.DrawPosition.X - nub.DrawWidth / 2 + 2, 0, innerWidth), 1);
                rightBox.Scale = new Vector2(MathHelper.Clamp(
                    innerWidth - nub.DrawPosition.X - nub.DrawWidth / 2 + 2, 0, innerWidth), 1);
            }

            protected override void UpdateValue(float value)
            {
                nub.MoveToX(innerWidth * value);
            }
        }
    }
}
