// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneModSelectScreen : OsuManualInputManagerTestScene
    {
        private ModSelectScreen modSelectScreen;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create screen", () => Child = modSelectScreen = new ModSelectScreen
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
                SelectedMods = { BindTarget = SelectedMods }
            });
        }

        [Test]
        public void TestStateChange()
        {
            AddToggleStep("toggle state", visible => modSelectScreen.State.Value = visible ? Visibility.Visible : Visibility.Hidden);
        }

        [Test]
        public void TestCustomisationToggleState()
        {
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select customisable mod", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod requiring configuration", () => SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("dismiss mod customisation", () =>
            {
                InputManager.MoveMouseTo(modSelectScreen.ChildrenOfType<ShearedToggleButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("append another mod not requiring config", () => SelectedMods.Value = SelectedMods.Value.Append(new OsuModFlashlight()).ToArray());
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select mod requiring configuration", () => SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false); // config was dismissed without explicit user action.
        }

        private void assertCustomisationToggleState(bool disabled, bool active)
        {
            ShearedToggleButton getToggle() => modSelectScreen.ChildrenOfType<ShearedToggleButton>().Single();

            AddAssert($"customisation toggle is {(disabled ? "" : "not ")}disabled", () => getToggle().Active.Disabled == disabled);
            AddAssert($"customisation toggle is {(active ? "" : "not ")}active", () => getToggle().Active.Value == active);
        }
    }
}
