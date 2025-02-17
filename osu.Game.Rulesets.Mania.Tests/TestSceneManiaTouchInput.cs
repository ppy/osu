// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaTouchInput : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestTouchInput()
        {
            for (int i = 0; i < 4; i++)
            {
                int index = i;

                AddStep($"touch column {index}", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, getColumn(index).ScreenSpaceDrawQuad.Centre)));

                AddAssert("action sent",
                    () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                    () => Does.Contain(getColumn(index).Action.Value));

                AddStep($"release column {index}", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, getColumn(index).ScreenSpaceDrawQuad.Centre)));

                AddAssert("action released",
                    () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                    () => Does.Not.Contain(getColumn(index).Action.Value));
            }
        }

        [Test]
        public void TestOneColumnMultipleTouches()
        {
            AddStep("touch column 0", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, getColumn(0).ScreenSpaceDrawQuad.Centre)));

            AddAssert("action sent",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Contain(getColumn(0).Action.Value));

            AddStep("touch another finger", () => InputManager.BeginTouch(new Touch(TouchSource.Touch2, getColumn(0).ScreenSpaceDrawQuad.Centre)));

            AddAssert("action still pressed",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Contain(getColumn(0).Action.Value));

            AddStep("release first finger", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, getColumn(0).ScreenSpaceDrawQuad.Centre)));

            AddAssert("action still pressed",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Contain(getColumn(0).Action.Value));

            AddStep("release second finger", () => InputManager.EndTouch(new Touch(TouchSource.Touch2, getColumn(0).ScreenSpaceDrawQuad.Centre)));

            AddAssert("action released",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Not.Contain(getColumn(0).Action.Value));
        }

        private Column getColumn(int index) => this.ChildrenOfType<Column>().ElementAt(index);
    }
}
