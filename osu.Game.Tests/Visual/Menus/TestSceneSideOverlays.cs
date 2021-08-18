// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Overlays;

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
            AddUntilStep("right screen offset applied", () => Game.ScreenOffsetContainer.X == SettingsPanel.WIDTH * TestOsuGame.SIDE_OVERLAY_OFFSET_RATIO);

            AddStep("hide settings", () => Game.Settings.Hide());
            AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
        }

        [Test]
        public void TestScreenOffsettingOnNotificationOverlay()
        {
            AddStep("open notifications", () => Game.Notifications.Show());
            AddUntilStep("right screen offset applied", () => Game.ScreenOffsetContainer.X == -NotificationOverlay.WIDTH * TestOsuGame.SIDE_OVERLAY_OFFSET_RATIO);

            AddStep("hide notifications", () => Game.Notifications.Hide());
            AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
        }
    }
}
