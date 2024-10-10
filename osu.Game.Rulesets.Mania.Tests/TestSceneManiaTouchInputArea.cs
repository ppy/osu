// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaTouchInputArea : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestTouchAreaNotInitiallyVisible()
        {
            AddAssert("touch area not visible", () => getTouchOverlay()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestPressReceptors()
        {
            AddAssert("touch area not visible", () => getTouchOverlay()?.State.Value == Visibility.Hidden);

            for (int i = 0; i < 4; i++)
            {
                int index = i;

                AddStep($"touch receptor {index}", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, getReceptor(index).ScreenSpaceDrawQuad.Centre)));

                AddAssert("action sent",
                    () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                    () => Does.Contain(getReceptor(index).Action.Value));

                AddStep($"release receptor {index}", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, getReceptor(index).ScreenSpaceDrawQuad.Centre)));

                AddAssert("touch area visible", () => getTouchOverlay()?.State.Value == Visibility.Visible);
            }
        }

        private ManiaTouchInputArea? getTouchOverlay() => this.ChildrenOfType<ManiaTouchInputArea>().SingleOrDefault();

        private ManiaTouchInputArea.ColumnInputReceptor getReceptor(int index) => this.ChildrenOfType<ManiaTouchInputArea.ColumnInputReceptor>().ElementAt(index);
    }
}
