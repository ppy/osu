// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneKeyCounter : OsuManualInputManagerTestScene
    {
        public TestSceneKeyCounter()
        {
            DefaultKeyCounter testCounter;
            KeyCounterDisplay kc;
            KeyCounterDisplay argonKc;

            Children = new Drawable[]
            {
                kc = new DefaultKeyCounterDisplay
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, -50),
                    Children = new[]
                    {
                        testCounter = new DefaultKeyCounter(new KeyCounterKeyboardTrigger(Key.X)),
                        new DefaultKeyCounter(new KeyCounterKeyboardTrigger(Key.X)),
                        new DefaultKeyCounter(new KeyCounterMouseTrigger(MouseButton.Left)),
                        new DefaultKeyCounter(new KeyCounterMouseTrigger(MouseButton.Right)),
                    },
                },
                argonKc = new ArgonKeyCounterDisplay
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, 50),
                    Children = new[]
                    {
                        new ArgonKeyCounter(new KeyCounterKeyboardTrigger(Key.X)),
                        new ArgonKeyCounter(new KeyCounterKeyboardTrigger(Key.X)),
                        new ArgonKeyCounter(new KeyCounterMouseTrigger(MouseButton.Left)),
                        new ArgonKeyCounter(new KeyCounterMouseTrigger(MouseButton.Right)),
                    },
                }
            };

            AddStep("Add random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                kc.Add(kc.CreateKeyCounter(new KeyCounterKeyboardTrigger(key)));
                argonKc.Add(argonKc.CreateKeyCounter(new KeyCounterKeyboardTrigger(key)));
            });

            Key testKey = ((KeyCounterKeyboardTrigger)kc.Children.First().Trigger).Key;

            void addPressKeyStep()
            {
                AddStep($"Press {testKey} key", () => InputManager.Key(testKey));
            }

            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses == 1);
            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses == 2);
            AddStep("Disable counting", () => testCounter.IsCounting = false);
            addPressKeyStep();
            AddAssert($"Check {testKey} count has not changed", () => testCounter.CountPresses == 2);
        }
    }
}
