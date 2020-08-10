// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneOverlayActivation : OsuPlayerTestScene
    {
        private OverlayTestPlayer testPlayer;

        public override void SetUpSteps()
        {
            AddStep("disable overlay activation during gameplay", () => LocalConfig.Set(OsuSetting.GameplayDisableOverlayActivation, true));
            base.SetUpSteps();
        }

        [Test]
        public void TestGameplayOverlayActivation()
        {
            AddAssert("activation mode is disabled", () => testPlayer.OverlayActivationMode == OverlayActivation.Disabled);
        }

        [Test]
        public void TestGameplayOverlayActivationDisabled()
        {
            AddStep("enable overlay activation during gameplay", () => LocalConfig.Set(OsuSetting.GameplayDisableOverlayActivation, false));
            AddAssert("activation mode is user triggered", () => testPlayer.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationPaused()
        {
            AddUntilStep("activation mode is disabled", () => testPlayer.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("pause gameplay", () => testPlayer.Pause());
            AddUntilStep("activation mode is user triggered", () => testPlayer.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationReplayLoaded()
        {
            AddAssert("activation mode is disabled", () => testPlayer.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("load a replay", () => testPlayer.DrawableRuleset.HasReplayLoaded.Value = true);
            AddAssert("activation mode is user triggered", () => testPlayer.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationBreaks()
        {
            AddAssert("activation mode is disabled", () => testPlayer.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("seek to break", () => testPlayer.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.Breaks.First().StartTime));
            AddUntilStep("activation mode is user triggered", () => testPlayer.OverlayActivationMode == OverlayActivation.UserTriggered);
            AddStep("seek to break end", () => testPlayer.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.Breaks.First().EndTime));
            AddUntilStep("activation mode is disabled", () => testPlayer.OverlayActivationMode == OverlayActivation.Disabled);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => testPlayer = new OverlayTestPlayer();

        private class OverlayTestPlayer : TestPlayer
        {
            public new OverlayActivation OverlayActivationMode => base.OverlayActivationMode.Value;
        }
    }
}
