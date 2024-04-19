// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Tests.Gameplay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinnableHUDOverlay : SkinnableTestScene
    {
        private HUDOverlay hudOverlay;

        [Cached(typeof(ScoreProcessor))]
        private ScoreProcessor scoreProcessor { get; set; }

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [Cached]
        private GameplayState gameplayState = TestGameplayState.Create(new OsuRuleset());

        [Cached(typeof(IGameplayClock))]
        private readonly IGameplayClock gameplayClock = new GameplayClockContainer(new TrackVirtual(60000), false, false);

        private IEnumerable<HUDOverlay> hudOverlays => CreatedDrawables.OfType<HUDOverlay>();

        // best way to check without exposing.
        private Drawable hideTarget => hudOverlay.ChildrenOfType<SkinnableContainer>().First();
        private Drawable keyCounterFlow => hudOverlay.ChildrenOfType<KeyCounterDisplay>().First().ChildrenOfType<FillFlowContainer<KeyCounter>>().Single();

        public TestSceneSkinnableHUDOverlay()
        {
            scoreProcessor = gameplayState.ScoreProcessor;
        }

        [Test]
        public void TestComboCounterIncrementing()
        {
            createNew();

            AddRepeatStep("increase combo", () => scoreProcessor.Combo.Value++, 10);

            AddStep("reset combo", () => scoreProcessor.Combo.Value = 0);
        }

        [Test]
        public void TestFadesInOnLoadComplete()
        {
            float? initialAlpha = null;

            createNew(h => h.OnLoadComplete += _ => initialAlpha = hideTarget.Alpha);
            AddAssert("initial alpha was less than 1", () => initialAlpha < 1);
        }

        [Test]
        public void TestHideExternally()
        {
            createNew();

            AddStep("set showhud false", () => hudOverlays.ForEach(h => h.ShowHud.Value = false));

            AddUntilStep("hidetarget is hidden", () => hideTarget.Alpha, () => Is.LessThanOrEqualTo(0));
            AddAssert("pause button is still visible", () => hudOverlay.HoldToQuit.IsPresent);

            // Key counter flow container should not be affected by this, only the key counter display will be hidden as checked above.
            AddAssert("key counter flow not affected", () => keyCounterFlow.IsPresent);
        }

        private void createNew(Action<HUDOverlay> action = null)
        {
            AddStep("create overlay", () =>
            {
                SetContents(_ =>
                {
                    hudOverlay = new HUDOverlay(null, Array.Empty<Mod>());

                    // Add any key just to display the key counter visually.
                    hudOverlay.InputCountController.Add(new KeyCounterKeyboardTrigger(Key.Space));

                    action?.Invoke(hudOverlay);

                    return hudOverlay;
                });
            });
            AddUntilStep("HUD overlay loaded", () => hudOverlay.IsAlive);
            AddUntilStep("components container loaded",
                () => hudOverlay.ChildrenOfType<SkinnableContainer>().Any(scc => scc.ComponentsLoaded));
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
