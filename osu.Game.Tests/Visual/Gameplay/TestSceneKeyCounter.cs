// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneKeyCounter : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(KeyCounterKeyboard),
            typeof(KeyCounterMouse),
            typeof(KeyCounterDisplay)
        };

        public TestSceneKeyCounter()
        {
            KeyCounterKeyboard testCounter;

            KeyCounterDisplay kc = new KeyCounterDisplay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new KeyCounter[]
                {
                    testCounter = new KeyCounterKeyboard(Key.X),
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

            Key testKey = ((KeyCounterKeyboard)kc.Children.First()).Key;

            AddStep($"Press {testKey} key", () =>
            {
                InputManager.PressKey(testKey);
                InputManager.ReleaseKey(testKey);
            });

            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses == 1);

            AddStep($"Press {testKey} key", () =>
            {
                InputManager.PressKey(testKey);
                InputManager.ReleaseKey(testKey);
            });

            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses == 2);

            Add(kc);
        }
    }
}
