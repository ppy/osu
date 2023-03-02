// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osu.Game.Tests.Gameplay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneHUDOverlay : OsuManualInputManagerTestScene
    {
        private OsuConfigManager localConfig = null!;

        private HUDOverlay hudOverlay = null!;

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new OsuRuleset());

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [Cached]
        private GameplayState gameplayState = TestGameplayState.Create(new OsuRuleset());

        [Cached(typeof(IGameplayClock))]
        private readonly IGameplayClock gameplayClock = new GameplayClockContainer(new FramedClock());

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
        public void TestHoldForMenuDoesWorkWhenHidden()
        {
            bool activated = false;

            HoldForMenuButton getHoldForMenu() => hudOverlay.ChildrenOfType<HoldForMenuButton>().Single();

            createNew();

            AddStep("bind action", () =>
            {
                activated = false;

                var holdForMenu = getHoldForMenu();

                holdForMenu.Action += () => activated = true;
            });

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);

            AddStep("attempt activate", () =>
            {
                InputManager.MoveMouseTo(getHoldForMenu().OfType<HoldToConfirmContainer>().Single());
                InputManager.PressButton(MouseButton.Left);
            });

            AddUntilStep("activated", () => activated);

            AddStep("release mouse button", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
            });
        }

        [Test]
        public void TestInputDoesntWorkWhenHUDHidden()
        {
            ArgonSongProgress? getSongProgress() => hudOverlay.ChildrenOfType<ArgonSongProgress>().SingleOrDefault();

            bool seeked = false;

            createNew();

            AddUntilStep("wait for song progress", () => getSongProgress() != null);

            AddStep("bind seek", () =>
            {
                seeked = false;

                var progress = getSongProgress();

                Debug.Assert(progress != null);

                progress.Interactive.Value = true;
                progress.ChildrenOfType<ArgonSongProgressBar>().Single().OnSeek += _ => seeked = true;
            });

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);

            AddStep("attempt seek", () =>
            {
                InputManager.MoveMouseTo(getSongProgress());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("seek not performed", () => !seeked);

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);

            AddStep("attempt seek", () => InputManager.Click(MouseButton.Left));
            AddAssert("seek performed", () => seeked);
        }

        [Test]
        public void TestHiddenHUDDoesntBlockComponentUpdates()
        {
            int updateCount = 0;

            AddStep("set hud to never show", () => localConfig.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Never));

            createNew();

            AddUntilStep("wait for hud load", () => hudOverlay.IsLoaded);
            AddUntilStep("wait for components to be hidden", () => hudOverlay.ChildrenOfType<SkinComponentsContainer>().Single().Alpha == 0);
            AddUntilStep("wait for hud load", () => hudOverlay.ChildrenOfType<SkinComponentsContainer>().All(c => c.ComponentsLoaded));

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
            AddUntilStep("wait for components to be hidden", () => hudOverlay.ChildrenOfType<SkinComponentsContainer>().Single().Alpha == 0);

            AddStep("reload components", () => hudOverlay.ChildrenOfType<SkinComponentsContainer>().Single().Reload());
            AddUntilStep("skinnable components loaded", () => hudOverlay.ChildrenOfType<SkinComponentsContainer>().Single().ComponentsLoaded);
        }

        private void createNew(Action<HUDOverlay>? action = null)
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
            if (localConfig.IsNotNull())
                localConfig.Dispose();

            base.Dispose(isDisposing);
        }
    }
}
