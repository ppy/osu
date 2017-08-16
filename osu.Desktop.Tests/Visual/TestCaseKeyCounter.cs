// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.MathUtils;
using osu.Game.Screens.Play;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseKeyCounter : OsuTestCase
    {
        public override string Description => @"Tests key counter";

        public TestCaseKeyCounter()
        {
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

            AddStep("Add random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                kc.Add(new KeyCounterKeyboard(key));
            });
            AddSliderStep("Fade time", 0, 200, 50, v => kc.FadeTime = v);

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
                    300, Easing.OutQuint);
            }
        }
    }
}
