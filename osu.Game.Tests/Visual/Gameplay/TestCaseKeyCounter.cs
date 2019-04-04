// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestCaseKeyCounter : ManualInputManagerTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(KeyCounterKeyboard),
            typeof(KeyCounterMouse),
            typeof(KeyCounterDisplay)
        };

        public TestCaseKeyCounter()
        {
            KeyCounterKeyboard rewindTestKeyCounterKeyboard;
            KeyCounterDisplay kc = new KeyCounterDisplay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new KeyCounter[]
                {
                    rewindTestKeyCounterKeyboard = new KeyCounterKeyboard(Key.X),
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

            Key testKey = ((KeyCounterKeyboard)kc.Children.First()).Key;
            double time1 = 0;

            AddStep($"Press {testKey} key", () =>
            {
                InputManager.PressKey(testKey);
                InputManager.ReleaseKey(testKey);
            });

            AddAssert($"Check {testKey} counter after keypress", () => rewindTestKeyCounterKeyboard.CountPresses == 1);

            AddStep($"Press {testKey} key", () =>
            {
                InputManager.PressKey(testKey);
                InputManager.ReleaseKey(testKey);
                time1 = Clock.CurrentTime;
            });

            AddAssert($"Check {testKey} counter after keypress", () => rewindTestKeyCounterKeyboard.CountPresses == 2);

            IFrameBasedClock oldClock = null;

            AddStep($"Rewind {testKey} counter once", () =>
            {
                oldClock = rewindTestKeyCounterKeyboard.Clock;
                rewindTestKeyCounterKeyboard.Clock = new FramedOffsetClock(new FixedClock(time1 - 10));
            });

            AddAssert($"Check {testKey} counter after rewind", () => rewindTestKeyCounterKeyboard.CountPresses == 1);

            AddStep($"Rewind {testKey} counter to zero", () => rewindTestKeyCounterKeyboard.Clock = new FramedOffsetClock(new FixedClock(0)));

            AddAssert($"Check {testKey} counter after rewind", () => rewindTestKeyCounterKeyboard.CountPresses == 0);

            AddStep("Restore clock", () => rewindTestKeyCounterKeyboard.Clock = oldClock);

            Add(kc);
        }

        private class FixedClock : IClock
        {
            private readonly double time;

            public FixedClock(double time)
            {
                this.time = time;
            }

            public double CurrentTime => time;
            public double Rate => 1;
            public bool IsRunning => false;
        }
    }
}
