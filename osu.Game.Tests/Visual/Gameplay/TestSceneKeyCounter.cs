// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneKeyCounter : OsuManualInputManagerTestScene
    {
        public TestSceneKeyCounter()
        {
            KeyCounterDisplay defaultDisplay = new DefaultKeyCounterDisplay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Position = new Vector2(0, 72.7f)
            };

            KeyCounterDisplay argonDisplay = new ArgonKeyCounterDisplay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Position = new Vector2(0, -72.7f)
            };

            defaultDisplay.AddRange(new InputTrigger[]
            {
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterMouseTrigger(MouseButton.Left),
                new KeyCounterMouseTrigger(MouseButton.Right),
            });

            argonDisplay.AddRange(new InputTrigger[]
            {
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterMouseTrigger(MouseButton.Left),
                new KeyCounterMouseTrigger(MouseButton.Right),
            });

            var testCounter = (DefaultKeyCounter)defaultDisplay.Counters.First();

            AddStep("Add random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                defaultDisplay.Add(new KeyCounterKeyboardTrigger(key));
                argonDisplay.Add(new KeyCounterKeyboardTrigger(key));
            });

            Key testKey = ((KeyCounterKeyboardTrigger)defaultDisplay.Counters.First().Trigger).Key;

            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses.Value == 1);
            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testCounter.CountPresses.Value == 2);
            AddStep("Disable counting", () =>
            {
                argonDisplay.IsCounting.Value = false;
                defaultDisplay.IsCounting.Value = false;
            });
            addPressKeyStep();
            AddAssert($"Check {testKey} count has not changed", () => testCounter.CountPresses.Value == 2);

            Add(defaultDisplay);
            Add(argonDisplay);

            void addPressKeyStep() => AddStep($"Press {testKey} key", () => InputManager.Key(testKey));
        }
    }
}
