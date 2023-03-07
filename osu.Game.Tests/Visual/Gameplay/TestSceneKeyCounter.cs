// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Screens.Play.HUD;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneKeyCounter : OsuManualInputManagerTestScene
    {
        public TestSceneKeyCounter()
        {
            KeyCounterDisplay kc = new DefaultKeyCounterDisplay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            };

            kc.AddTriggerRange(new InputTrigger[]
            {
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterMouseTrigger(MouseButton.Left),
                new KeyCounterMouseTrigger(MouseButton.Right),
            });

            var testCounter = (DefaultKeyCounter)kc.Children.First();

            AddStep("Add random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                kc.AddTrigger(new KeyCounterKeyboardTrigger(key));
            });

            Key testKey = ((KeyCounterKeyboardTrigger)kc.Children.First().Trigger).Key;

            void addPressKeyStep()
            {
                AddStep($"Press {testKey} key", () => InputManager.Key(testKey));
            }

            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses.Value == 1);
            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses.Value == 2);
            AddStep("Disable counting", () => testCounter.IsCounting.Value = false);
            addPressKeyStep();
            AddAssert($"Check {testKey} count has not changed", () => testCounter.CountPresses.Value == 2);

            Add(kc);
        }
    }
}
