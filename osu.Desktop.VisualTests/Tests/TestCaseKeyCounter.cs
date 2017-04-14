// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Framework.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseKeyCounter : TestCase
    {
        public override string Description => @"Tests key counter";

        public override void Reset()
        {
            base.Reset();

            KeyCounterCollection kc = new KeyCounterCollection
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                IsCounting = true,
                Children = new KeyCounter[]
                {
                    new KeyCounterKeyboard(Key.Z),
                    new KeyCounterKeyboard(Key.X),
                    new KeyCounterMouse(MouseButton.Left),
                    new KeyCounterMouse(MouseButton.Right),
                },
            };
            BindableInt bindable = new BindableInt { MinValue = 0, MaxValue = 200, Default = 50 };
            bindable.ValueChanged += delegate { kc.FadeTime = bindable.Value; };
            AddStep("Add Random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                kc.Add(new KeyCounterKeyboard(key));
            });

            TestSliderBar<int> sliderBar;

            Add(new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SpriteText { Text = "FadeTime" },
                    sliderBar =new TestSliderBar<int>
                    {
                        Width = 150,
                        Height = 10,
                        SelectionColor = Color4.Orange,
                    }
                }
            });

            sliderBar.Current.BindTo(bindable);

            Add(kc);
        }
        private class TestSliderBar<T> : SliderBar<T> where T : struct
        {
            public Color4 Color
            {
                get { return Box.Colour; }
                set { Box.Colour = value; }
            }

            public Color4 SelectionColor
            {
                get { return SelectionBox.Colour; }
                set { SelectionBox.Colour = value; }
            }

            protected readonly Box SelectionBox;
            protected readonly Box Box;

            public TestSliderBar()
            {
                Children = new Drawable[]
                {
                    Box = new Box { RelativeSizeAxes = Axes.Both },
                    SelectionBox = new Box { RelativeSizeAxes = Axes.Both }
                };
            }

            protected override void UpdateValue(float value)
            {
                SelectionBox.ScaleTo(
                    new Vector2(value, 1),
                    300, EasingTypes.OutQuint);
            }
        }
    }
}
