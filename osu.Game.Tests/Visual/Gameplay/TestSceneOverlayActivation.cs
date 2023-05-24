// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneOverlayActivation : OsuPlayerTestScene
    {
        protected new OverlayTestPlayer Player => base.Player as OverlayTestPlayer;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("gameplay has started",
                () => Player.GameplayClockContainer.CurrentTime > Player.DrawableRuleset.GameplayStartTime);
        }

        [Test]
        public void TestGameplayOverlayActivation()
        {
            AddAssert("local user playing", () => Player.LocalUserPlaying.Value);
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
        }

        [Test]
        public void TestGameplayOverlayActivationPaused()
        {
            AddAssert("local user playing", () => Player.LocalUserPlaying.Value);
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("pause gameplay", () => Player.Pause());
            AddAssert("local user not playing", () => !Player.LocalUserPlaying.Value);
            AddUntilStep("activation mode is user triggered", () => Player.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationReplayLoaded()
        {
            AddAssert("local user playing", () => Player.LocalUserPlaying.Value);
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("load a replay", () => Player.DrawableRuleset.HasReplayLoaded.Value = true);
            AddAssert("local user not playing", () => !Player.LocalUserPlaying.Value);
            AddAssert("activation mode is user triggered", () => Player.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationBreaks()
        {
            AddAssert("local user playing", () => Player.LocalUserPlaying.Value);
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("seek to break", () => Player.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.Breaks.First().StartTime));
            AddUntilStep("activation mode is user triggered", () => Player.OverlayActivationMode == OverlayActivation.UserTriggered);
            AddAssert("local user not playing", () => !Player.LocalUserPlaying.Value);
            AddStep("seek to break end", () => Player.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.Breaks.First().EndTime));
            AddUntilStep("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddAssert("local user playing", () => Player.LocalUserPlaying.Value);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new OverlayTestPlayer();

        protected partial class OverlayTestPlayer : TestPlayer
        {
            public new OverlayActivation OverlayActivationMode => base.OverlayActivationMode.Value;
        }
    }
}
