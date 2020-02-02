// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHUDOverlay : ManualInputManagerTestScene
    {
        private TestHUDOverlay hudOverlay;

        private Drawable hideTarget => hudOverlay.KeyCounter; // best way of checking hideTargets without exposing.

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestShownByDefault()
        {
            createNew();

            AddAssert("showhud is set", () => hudOverlay.ShowHud.Value);

            AddAssert("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counter flow is visible", () => hudOverlay.KeyCounter.KeyFlow.IsPresent);
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

            // Key counter flow container should not be affected by this, only the key counter display will be hidden as checked above.
            AddAssert("key counter flow not affected", () => hudOverlay.KeyCounter.KeyFlow.IsPresent);
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

        [Test]
        public void TestChangeHUDVisibilityOnHiddenKeyCounter()
        {
            bool keyCounterVisibleValue = false;

            createNew();
            AddStep("save keycounter visible value", () => keyCounterVisibleValue = config.Get<bool>(OsuSetting.KeyOverlay));

            AddStep("set keycounter visible false", () =>
            {
                config.Set<bool>(OsuSetting.KeyOverlay, false);
                hudOverlay.KeyCounter.Visible.Value = false;
            });

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("key counters hidden", () => !hudOverlay.KeyCounter.KeyFlow.IsPresent);

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddUntilStep("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counters still hidden", () => !hudOverlay.KeyCounter.KeyFlow.IsPresent);

            AddStep("return value", () => config.Set<bool>(OsuSetting.KeyOverlay, keyCounterVisibleValue));
        }

        private void createNew(Action<TestHUDOverlay> action = null)
        {
            AddStep("create overlay", () =>
            {
                Child = hudOverlay = new TestHUDOverlay();

                action?.Invoke(hudOverlay);
            });
        }

        private class TestHUDOverlay : HUDOverlay
        {
            public new TestKeyCounterDisplay KeyCounter => (TestKeyCounterDisplay)base.KeyCounter;

            protected override KeyCounterDisplay CreateKeyCounter() => new TestKeyCounterDisplay
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(10),
            };

            public TestHUDOverlay()
                : base(null, null, null, Array.Empty<Mod>())
            {
                // Add any key just to display the key counter visually.
                KeyCounter.Add(new KeyCounterKeyboard(Key.Space));
            }
        }

        private class TestKeyCounterDisplay : KeyCounterDisplay
        {
            public new FillFlowContainer<KeyCounter> KeyFlow => base.KeyFlow;
        }
    }
}
