// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Tests.Visual.Navigation;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneSideOverlays : OsuGameTestScene
    {
        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddAssert("no screen offset applied", () => Game.ScreenOffsetContainer.X == 0f);
            AddUntilStep("wait for overlays", () => Game.Settings.IsLoaded && Game.Notifications.IsLoaded);
        }

        [Test]
        public void TestScreenOffsettingOnSettingsOverlay()
        {
            AddStep("open settings", () => Game.Settings.Show());
            AddUntilStep("right screen offset applied", () => Game.ScreenOffsetContainer.X == SettingsPanel.WIDTH * OsuGame.SCREEN_OFFSET_RATIO);

            AddStep("hide settings", () => Game.Settings.Hide());
            AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
        }

        [Test]
        public void TestScreenOffsettingAccountsForKeyBindingPanel()
        {
            AddStep("open settings", () => Game.Settings.Show());
            AddStep("open key binding panel", () => Game.Settings.ChildrenOfType<KeyBindingPanel>().Single().Show());
            AddUntilStep("right screen offset applied", () => Game.ScreenOffsetContainer.X == SettingsPanel.WIDTH * OsuGame.SCREEN_OFFSET_RATIO);

            AddStep("hide key binding", () => Game.Settings.ChildrenOfType<KeyBindingPanel>().Single().Show());
            AddUntilStep("right screen offset still applied", () => Game.ScreenOffsetContainer.X == SettingsPanel.WIDTH * OsuGame.SCREEN_OFFSET_RATIO);

            AddStep("open key binding", () => Game.Settings.Show());
            AddUntilStep("right screen offset still applied", () => Game.ScreenOffsetContainer.X == SettingsPanel.WIDTH * OsuGame.SCREEN_OFFSET_RATIO);

            AddStep("hide settings", () => Game.Settings.Hide());
            AddAssert("key binding panel still open", () => Game.Settings.ChildrenOfType<KeyBindingPanel>().Single().State.Value == Visibility.Visible);
            AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
        }

        [Test]
        public void TestScreenOffsettingOnNotificationOverlay()
        {
            AddStep("open notifications", () => Game.Notifications.Show());
            AddUntilStep("right screen offset applied", () => Game.ScreenOffsetContainer.X == -NotificationOverlay.WIDTH * OsuGame.SCREEN_OFFSET_RATIO);

            AddStep("hide notifications", () => Game.Notifications.Hide());
            AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
        }
    }
}
