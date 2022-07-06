// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osu.Game.Tests.Gameplay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHUDOverlay : OsuManualInputManagerTestScene
    {
        private OsuConfigManager localConfig;

        private HUDOverlay hudOverlay;

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new OsuRuleset());

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [Cached]
        private GameplayState gameplayState = TestGameplayState.Create(new OsuRuleset());

        [Cached]
        private readonly GameplayClock gameplayClock = new GameplayClock(new FramedClock());

        // best way to check without exposing.
        private Drawable hideTarget => hudOverlay.KeyCounter;
        private FillFlowContainer<KeyCounter> keyCounterFlow => hudOverlay.KeyCounter.ChildrenOfType<FillFlowContainer<KeyCounter>>().First();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [SetUp]
        public void SetUp() => Schedule(() => localConfig.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always));

        [Test]
        public void TestComboCounterIncrementing()
        {
            createNew();

            AddRepeatStep("increase combo", () => { scoreProcessor.Combo.Value++; }, 10);

            AddStep("reset combo", () => { scoreProcessor.Combo.Value = 0; });
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

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);

            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("pause button is still visible", () => hudOverlay.HoldToQuit.IsPresent);

            // Key counter flow container should not be affected by this, only the key counter display will be hidden as checked above.
            AddAssert("key counter flow not affected", () => keyCounterFlow.IsPresent);
        }

        [Test]
        public void TestMomentaryShowHUD()
        {
            createNew();

            AddStep("set hud to never show", () => localConfig.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Never));

            AddUntilStep("wait for fade", () => !hideTarget.IsPresent);

            AddStep("trigger momentary show", () => InputManager.PressKey(Key.ControlLeft));
            AddUntilStep("wait for visible", () => hideTarget.IsPresent);

            AddStep("stop trigering", () => InputManager.ReleaseKey(Key.ControlLeft));
            AddUntilStep("wait for fade", () => !hideTarget.IsPresent);
        }

        [Test]
        public void TestExternalHideDoesntAffectConfig()
        {
            createNew();

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddAssert("config unchanged", () => localConfig.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode).IsDefault);

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddAssert("config unchanged", () => localConfig.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode).IsDefault);
        }

        [Test]
        public void TestChangeHUDVisibilityOnHiddenKeyCounter()
        {
            createNew();

            AddStep("hide key overlay", () =>
            {
                localConfig.SetValue(OsuSetting.KeyOverlay, false);
                hudOverlay.KeyCounter.AlwaysVisible.Value = false;
            });

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("key counters hidden", () => !keyCounterFlow.IsPresent);

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddUntilStep("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counters still hidden", () => !keyCounterFlow.IsPresent);
        }

        [Test]
        public void TestHiddenHUDDoesntBlockComponentUpdates()
        {
            int updateCount = 0;

            AddStep("set hud to never show", () => localConfig.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Never));

            createNew();

            AddUntilStep("wait for hud load", () => hudOverlay.IsLoaded);
            AddUntilStep("wait for components to be hidden", () => hudOverlay.ChildrenOfType<SkinnableTargetContainer>().Single().Alpha == 0);

            AddStep("bind on update", () =>
            {
                hudOverlay.ChildrenOfType<BarHitErrorMeter>().First().OnUpdate += _ => updateCount++;
            });

            AddUntilStep("wait for updates", () => updateCount > 0);
        }

        [Test]
        public void TestHiddenHUDDoesntBlockSkinnableComponentsLoad()
        {
            AddStep("set hud to never show", () => localConfig.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Never));

            createNew();

            AddUntilStep("wait for hud load", () => hudOverlay.IsLoaded);
            AddUntilStep("wait for components to be hidden", () => hudOverlay.ChildrenOfType<SkinnableTargetContainer>().Single().Alpha == 0);

            AddStep("reload components", () => hudOverlay.ChildrenOfType<SkinnableTargetContainer>().Single().Reload());
            AddUntilStep("skinnable components loaded", () => hudOverlay.ChildrenOfType<SkinnableTargetContainer>().Single().ComponentsLoaded);
        }

        private void createNew(Action<HUDOverlay> action = null)
        {
            AddStep("create overlay", () =>
            {
                hudOverlay = new HUDOverlay(null, Array.Empty<Mod>());

                // Add any key just to display the key counter visually.
                hudOverlay.KeyCounter.Add(new KeyCounterKeyboard(Key.Space));

                scoreProcessor.Combo.Value = 1;

                action?.Invoke(hudOverlay);

                Child = hudOverlay;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            localConfig?.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
