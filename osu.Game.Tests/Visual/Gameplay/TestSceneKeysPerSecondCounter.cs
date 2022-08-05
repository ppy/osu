// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.KPSCounter;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneKeysPerSecondCounter : OsuManualInputManagerTestScene
    {
        private KeysPerSecondCounter counter;

        [SetUpSteps]
        public void Setup()
        {
            createCounter();
        }

        private void createCounter() => AddStep("Create counter", () =>
        {
            Child = counter = new KeysPerSecondCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(5)
            };
        });

        [Test]
        public void TestManualTrigger()
        {
            AddAssert("Counter = 0", () => counter.Current.Value == 0);
            AddRepeatStep("manual trigger", KeysPerSecondCalculator.AddInput, 20);
            AddAssert("Counter is not 0", () => counter.Current.Value > 0);
        }

        [Test]
        public void TestKpsAsideKeyCounter()
        {
            AddStep("Create key counter display", () =>
                Add(new KeyCounterDisplay
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Y = 100,
                    Children = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(Key.W),
                        new KeyCounterKeyboard(Key.X),
                        new KeyCounterKeyboard(Key.C),
                        new KeyCounterKeyboard(Key.V)
                    }
                })
            );
            AddAssert("Counter = 0", () => counter.Current.Value == 0);
            addPressKeyStep(Key.W);
            addPressKeyStep(Key.X);
            addPressKeyStep(Key.C);
            addPressKeyStep(Key.V);
            AddAssert("Counter = 4", () => counter.Current.Value == 4);
        }

        private void addPressKeyStep(Key key)
        {
            AddStep($"Press {key} key", () => InputManager.Key(key));
        }
    }
}
