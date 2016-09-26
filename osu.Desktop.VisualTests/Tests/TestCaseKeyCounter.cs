//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Input;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.Tests
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
                IsCounting = true
            };
            Add(kc);
            kc.AddKey(new KeyCounterKeyboard(@"Z", Key.Z));
            kc.AddKey(new KeyCounterKeyboard(@"X", Key.X));
            kc.AddKey(new KeyCounterMouse(@"M1", MouseButton.Left));
            kc.AddKey(new KeyCounterMouse(@"M2", MouseButton.Right));
        }
    }
}
