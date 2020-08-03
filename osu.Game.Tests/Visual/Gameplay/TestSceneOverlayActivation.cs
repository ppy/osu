// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneOverlayActivation : OsuPlayerTestScene
    {
        private OverlayTestPlayer testPlayer;

        [Resolved]
        private OsuConfigManager config { get; set; }

        public override void SetUpSteps()
        {
            AddStep("disable overlay activation during gameplay", () => config.Set(OsuSetting.GameplayDisableOverlayActivation, true));
            base.SetUpSteps();
        }

        [Test]
        public void TestGameplayOverlayActivationSetting()
        {
            AddAssert("activation mode is disabled", () => testPlayer.OverlayActivationMode == OverlayActivation.Disabled);
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

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => testPlayer = new OverlayTestPlayer();

        private class OverlayTestPlayer : TestPlayer
        {
            public new OverlayActivation OverlayActivationMode => base.OverlayActivationMode.Value;
        }
    }
}
