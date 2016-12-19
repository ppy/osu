//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Configuration;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseKeyCounter : TestCase
    {
        public override string Name => @"KeyCounter";

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
                    new KeyCounterKeyboard(@"Z", Key.Z),
                    new KeyCounterKeyboard(@"X", Key.X),
                    new KeyCounterMouse(@"M1", MouseButton.Left),
                    new KeyCounterMouse(@"M2", MouseButton.Right),
                },
            };
            BindableInt bindable = new BindableInt { MinValue = 0, MaxValue = 200, Default = 50 };
            bindable.ValueChanged += delegate { kc.FadeTime = bindable.Value; };
            AddButton("Add Random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                kc.Add(new KeyCounterKeyboard(key.ToString(), key));
            });
            ButtonsContainer.Add(new SpriteText { Text = "FadeTime" });
            ButtonsContainer.Add(new TestSliderBar<int>
            {
                Width = 150,
                Height = 10,
                SelectionColor = Color4.Orange,
                Bindable = bindable
            });
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
