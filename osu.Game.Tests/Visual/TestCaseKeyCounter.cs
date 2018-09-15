// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input.EventArgs;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using OpenTK.Input;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseKeyCounter : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(KeyCounterKeyboard),
            typeof(KeyCounterMouse),
            typeof(KeyCounterCollection)
        };

        public TestCaseKeyCounter()
        {
            KeyCounterKeyboard rewindTestKeyCounterKeyboard;
            KeyCounterCollection kc = new KeyCounterCollection
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new KeyCounter[]
                {
                    rewindTestKeyCounterKeyboard = new KeyCounterKeyboard(rewind_test_key),
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

            var expectedCountPresses = rewindTestKeyCounterKeyboard.CountPresses + 1;
            AddStep($"Press {rewind_test_key} key", () =>
            {
                rewindTestKeyCounterKeyboard.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = rewind_test_key, Repeat = false });
                rewindTestKeyCounterKeyboard.TriggerOnKeyUp(null, new KeyUpEventArgs { Key = rewind_test_key });
            });

            AddAssert($"Check {rewind_test_key} counter after keypress", () => rewindTestKeyCounterKeyboard.CountPresses == expectedCountPresses);

            IFrameBasedClock counterClock = null;
            AddStep($"Rewind {rewind_test_key} counter", () =>
            {
                counterClock = rewindTestKeyCounterKeyboard.Clock;
                rewindTestKeyCounterKeyboard.Clock = new DecoupleableInterpolatingFramedClock();
            });

            AddAssert($"Check {rewind_test_key} counter after rewind", () =>
            {
                rewindTestKeyCounterKeyboard.Clock = counterClock;
                return rewindTestKeyCounterKeyboard.CountPresses == 0;
            });

            Add(kc);
        }
    }
}
