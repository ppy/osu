// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest] // we alter unsafe properties on the game host to test inactive window state.
    public class TestScenePauseWhenInactive : PlayerTestScene
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = (Beatmap)base.CreateBeatmap(ruleset);

            beatmap.HitObjects.RemoveAll(h => h.StartTime < 30000);

            return beatmap;
        }

        [Resolved]
        private GameHost host { get; set; }

        public TestScenePauseWhenInactive()
            : base(new OsuRuleset())
        {
        }

        [Test]
        public void TestDoesntPauseDuringIntro()
        {
            AddStep("set inactive", () => ((Bindable<bool>)host.IsActive).Value = false);

            AddStep("resume player", () => Player.GameplayClockContainer.Start());
            AddAssert("ensure not paused", () => !Player.GameplayClockContainer.IsPaused.Value);
            AddUntilStep("wait for pause", () => Player.GameplayClockContainer.IsPaused.Value);
            AddAssert("time of pause is after gameplay start time", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= Player.DrawableRuleset.GameplayStartTime);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(true, true, true);
    }
}
