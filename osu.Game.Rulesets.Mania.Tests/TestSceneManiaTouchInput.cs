// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaTouchInput : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [SetUp]
        public void SetUp() => Schedule(() => toggleTouchControls(false));

        #region Without touch controls

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

        #endregion

        #region With touch controls

        [Test]
        public void TestTouchAreaNotInitiallyVisible()
        {
            AddStep("enable touch controls", () => toggleTouchControls(true));
            AddAssert("touch area not visible", () => getTouchOverlay()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestPressReceptors()
        {
            AddStep("enable touch controls", () => toggleTouchControls(true));
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

        [Test]
        public void TestColumnsNotTouchableWithTouchControls()
        {
            AddStep("enable touch controls", () => toggleTouchControls(true));

            AddStep("touch receptor 0", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, getReceptor(0).ScreenSpaceDrawQuad.Centre)));

            AddAssert("action sent",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Contain(getReceptor(0).Action.Value));

            AddStep("release receptor 0", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, getReceptor(0).ScreenSpaceDrawQuad.Centre)));

            AddAssert("touch area visible", () => getTouchOverlay()?.State.Value == Visibility.Visible);

            AddStep("touch column 0", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, getColumn(0).ScreenSpaceDrawQuad.Centre + new Vector2(0f, -50f))));

            AddAssert("action not sent",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Not.Contain(getColumn(0).Action.Value));

            AddStep("release column 0", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, getColumn(0).ScreenSpaceDrawQuad.Centre + new Vector2(0f, -50f))));

            AddAssert("action not sent",
                () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                () => Does.Not.Contain(getColumn(0).Action.Value));
        }

        #endregion

        private void toggleTouchControls(bool enabled)
        {
            var maniaConfig = (ManiaRulesetConfigManager)RulesetConfigs.GetConfigFor(CreatePlayerRuleset())!;
            maniaConfig.SetValue(ManiaRulesetSetting.MobileLayout, enabled ? ManiaMobileLayout.LandscapeWithOverlay : ManiaMobileLayout.Portrait);
        }

        private ManiaTouchInputArea? getTouchOverlay() => this.ChildrenOfType<ManiaTouchInputArea>().SingleOrDefault();

        private ManiaTouchInputArea.ColumnInputReceptor getReceptor(int index) => this.ChildrenOfType<ManiaTouchInputArea.ColumnInputReceptor>().ElementAt(index);

        private Column getColumn(int index) => this.ChildrenOfType<Column>().ElementAt(index);
    }
}
