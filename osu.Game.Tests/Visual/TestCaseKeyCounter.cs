// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Screens.Play;
using OpenTK.Input;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseKeyCounter : OsuTestCase
    {
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
    }
}
