// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneSideOverlays : OsuGameTestScene
    {
        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddAssert("no screen offset applied", () => Game.ScreenOffsetContainer.X == 0f);

            // avoids mouse interacting with settings overlay.
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));

            AddUntilStep("wait for overlays", () => Game.Settings.IsLoaded && Game.Notifications.IsLoaded);
        }

        [Test]
        public void TestScreenOffsettingOnSettingsOverlay()
        {
            foreach (var scalingMode in Enum.GetValues(typeof(ScalingMode)).Cast<ScalingMode>())
            {
                AddStep($"set scaling mode to {scalingMode}", () =>
                {
                    Game.LocalConfig.SetValue(OsuSetting.Scaling, scalingMode);

                    if (scalingMode != ScalingMode.Off)
                    {
                        Game.LocalConfig.SetValue(OsuSetting.ScalingSizeX, 0.5f);
                        Game.LocalConfig.SetValue(OsuSetting.ScalingSizeY, 0.5f);
                    }
                });

                AddStep("open settings", () => Game.Settings.Show());
                AddUntilStep("right screen offset applied", () => Precision.AlmostEquals(Game.ScreenOffsetContainer.X, SettingsPanel.WIDTH * TestOsuGame.SIDE_OVERLAY_OFFSET_RATIO));

                AddStep("hide settings", () => Game.Settings.Hide());
                AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
            }
        }

        [Test]
        public void TestScreenOffsettingOnNotificationOverlay()
        {
            foreach (var scalingMode in Enum.GetValues(typeof(ScalingMode)).Cast<ScalingMode>())
            {
                if (scalingMode != ScalingMode.Off)
                {
                    AddStep($"set scaling mode to {scalingMode}", () =>
                    {
                        Game.LocalConfig.SetValue(OsuSetting.Scaling, scalingMode);
                        Game.LocalConfig.SetValue(OsuSetting.ScalingSizeX, 0.5f);
                        Game.LocalConfig.SetValue(OsuSetting.ScalingSizeY, 0.5f);
                    });
                }

                AddStep("open notifications", () => Game.Notifications.Show());
                AddUntilStep("right screen offset applied", () => Precision.AlmostEquals(Game.ScreenOffsetContainer.X, -NotificationOverlay.WIDTH * TestOsuGame.SIDE_OVERLAY_OFFSET_RATIO));

                AddStep("hide notifications", () => Game.Notifications.Hide());
                AddUntilStep("screen offset removed", () => Game.ScreenOffsetContainer.X == 0f);
            }
        }
    }
}
