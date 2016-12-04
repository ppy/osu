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
            BindableInt bindable = new BindableInt { MinValue = 0, MaxValue = 1000, Default = 50 };
            AddButton("Add Random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                kc.Add(new KeyCounterKeyboard(key.ToString(), key));
            });
            Add(new SliderBar<int>
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Width = 150,
                Height = 10,
                SelectionColor = Color4.Orange,
                Position = new Vector2(0, 50),
                Bindable = bindable
            });
            Add(kc);
        }
    }
}
