// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHUDOverlay : ManualInputManagerTestScene
    {
        private HUDOverlay hudOverlay;

        private Drawable hideTarget => hudOverlay.KeyCounter; // best way of checking hideTargets without exposing.

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestShownByDefault()
        {
            createNew();

            AddAssert("showhud is set", () => hudOverlay.ShowHud.Value);

            AddAssert("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("pause button is visible", () => hudOverlay.HoldToQuit.IsPresent);
        }

        [Test]
        public void TestFadesInOnLoadComplete()
        {
            float? initialAlpha = null;

            createNew(h => h.OnLoadComplete += _ => initialAlpha = hideTarget.Alpha);
            AddUntilStep("wait for load", () => hudOverlay.IsAlive);
            AddAssert("initial alpha was less than 1", () => initialAlpha != null && initialAlpha < 1);
        }

        [Test]
        public void TestHideExternally()
        {
            createNew();

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);

            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("pause button is still visible", () => hudOverlay.HoldToQuit.IsPresent);
        }

        [Test]
        public void TestExternalHideDoesntAffectConfig()
        {
            bool originalConfigValue = false;

            createNew();

            AddStep("get original config value", () => originalConfigValue = config.Get<bool>(OsuSetting.ShowInterface));

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddAssert("config unchanged", () => originalConfigValue == config.Get<bool>(OsuSetting.ShowInterface));

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddAssert("config unchanged", () => originalConfigValue == config.Get<bool>(OsuSetting.ShowInterface));
        }

        private void createNew(Action<HUDOverlay> action = null)
        {
            AddStep("create overlay", () =>
            {
                Child = hudOverlay = new HUDOverlay(null, null, null, Array.Empty<Mod>());

                action?.Invoke(hudOverlay);
            });
        }
    }
}
