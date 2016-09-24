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

            KeyCounter kc = new KeyCounter
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                IsCounting = true
            };
            Add(kc);
            kc.AddKey(new KeyBoardCount(@"Z", Key.Z));
            kc.AddKey(new KeyBoardCount(@"X", Key.X));
            kc.AddKey(new MouseCount(@"M1", MouseButton.Left));
            kc.AddKey(new MouseCount(@"M2", MouseButton.Right));
        }
    }
}
