// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHUDOverlay : SkinnableTestScene
    {
        private HUDOverlay hudOverlay;

        private IEnumerable<HUDOverlay> hudOverlays => CreatedDrawables.OfType<HUDOverlay>();

        // best way to check without exposing.
        private Drawable hideTarget => hudOverlay.KeyCounter;
        private FillFlowContainer<KeyCounter> keyCounterFlow => hudOverlay.KeyCounter.ChildrenOfType<FillFlowContainer<KeyCounter>>().First();

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestComboCounterIncrementing()
        {
            createNew();

            AddRepeatStep("increase combo", () =>
            {
                foreach (var hud in hudOverlays)
                    hud.ComboCounter.Current.Value++;
            }, 10);

            AddStep("reset combo", () =>
            {
                foreach (var hud in hudOverlays)
                    hud.ComboCounter.Current.Value = 0;
            });
        }

        [Test]
        public void TestShownByDefault()
        {
            createNew();

            AddAssert("showhud is set", () => hudOverlay.ShowHud.Value);

            AddAssert("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counter flow is visible", () => keyCounterFlow.IsPresent);
            AddAssert("pause button is visible", () => hudOverlay.HoldToQuit.IsPresent);
        }

        [Test]
        public void TestFadesInOnLoadComplete()
        {
            float? initialAlpha = null;

            createNew(h => h.OnLoadComplete += _ => initialAlpha = hideTarget.Alpha);
            AddUntilStep("wait for load", () => hudOverlay.IsAlive);
            AddAssert("initial alpha was less than 1", () => initialAlpha < 1);
        }

        [Test]
        public void TestHideExternally()
        {
            createNew();

            AddStep("set showhud false", () => hudOverlays.ForEach(h => h.ShowHud.Value = false));

            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("pause button is still visible", () => hudOverlay.HoldToQuit.IsPresent);

            // Key counter flow container should not be affected by this, only the key counter display will be hidden as checked above.
            AddAssert("key counter flow not affected", () => keyCounterFlow.IsPresent);
        }

        [Test]
        public void TestExternalHideDoesntAffectConfig()
        {
            HUDVisibilityMode originalConfigValue = HUDVisibilityMode.HideDuringBreaks;

            createNew();

            AddStep("get original config value", () => originalConfigValue = config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddAssert("config unchanged", () => originalConfigValue == config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddAssert("config unchanged", () => originalConfigValue == config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));
        }

        [Test]
        public void TestChangeHUDVisibilityOnHiddenKeyCounter()
        {
            bool keyCounterVisibleValue = false;

            createNew();
            AddStep("save keycounter visible value", () => keyCounterVisibleValue = config.Get<bool>(OsuSetting.KeyOverlay));

            AddStep("set keycounter visible false", () =>
            {
                config.Set<bool>(OsuSetting.KeyOverlay, false);
                hudOverlays.ForEach(h => h.KeyCounter.AlwaysVisible.Value = false);
            });

            AddStep("set showhud false", () => hudOverlays.ForEach(h => h.ShowHud.Value = false));
            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("key counters hidden", () => !keyCounterFlow.IsPresent);

            AddStep("set showhud true", () => hudOverlays.ForEach(h => h.ShowHud.Value = true));
            AddUntilStep("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counters still hidden", () => !keyCounterFlow.IsPresent);

            AddStep("return value", () => config.Set<bool>(OsuSetting.KeyOverlay, keyCounterVisibleValue));
        }

        private void createNew(Action<HUDOverlay> action = null)
        {
            AddStep("create overlay", () =>
            {
                SetContents(() =>
                {
                    hudOverlay = new HUDOverlay(null, null, null, Array.Empty<Mod>());

                    // Add any key just to display the key counter visually.
                    hudOverlay.KeyCounter.Add(new KeyCounterKeyboard(Key.Space));

                    hudOverlay.ComboCounter.Current.Value = 1;

                    action?.Invoke(hudOverlay);

                    return hudOverlay;
                });
            });
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
