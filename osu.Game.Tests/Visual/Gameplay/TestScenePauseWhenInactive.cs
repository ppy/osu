// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest] // we alter unsafe properties on the game host to test inactive window state.
    public partial class TestScenePauseWhenInactive : OsuPlayerTestScene
    {
        [Resolved]
        private GameHost host { get; set; }

        [Test]
        public void TestDoesntPauseDuringIntro()
        {
            AddStep("set inactive", () => ((Bindable<bool>)host.IsActive).Value = false);

            AddStep("resume player", () => Player.GameplayClockContainer.Start());
            AddAssert("ensure not paused", () => !Player.GameplayClockContainer.IsPaused.Value);

            AddStep("progress time to gameplay", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.GameplayStartTime));
            AddUntilStep("wait for pause", () => Player.GameplayClockContainer.IsPaused.Value);
        }

        /// <summary>
        /// Tests that if a pause from focus lose is performed while in pause cooldown,
        /// the player will still pause after the cooldown is finished.
        /// </summary>
        [Test]
        public void TestPauseWhileInCooldown()
        {
            AddStep("move cursor outside", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft - new Vector2(10)));

            AddStep("resume player", () => Player.GameplayClockContainer.Start());
            AddStep("skip to gameplay", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.GameplayStartTime));

            AddStep("set inactive", () => ((Bindable<bool>)host.IsActive).Value = false);
            AddUntilStep("wait for pause", () => Player.GameplayClockContainer.IsPaused.Value);

            AddStep("set active", () => ((Bindable<bool>)host.IsActive).Value = true);

            AddStep("resume player", () => Player.Resume());
            AddAssert("unpaused", () => !Player.GameplayClockContainer.IsPaused.Value);

            bool pauseCooldownActive = false;

            AddStep("set inactive again", () =>
            {
                pauseCooldownActive = Player.PauseCooldownActive;
                ((Bindable<bool>)host.IsActive).Value = false;
            });
            AddAssert("pause cooldown active", () => pauseCooldownActive);
            AddUntilStep("wait for pause", () => Player.GameplayClockContainer.IsPaused.Value);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(true, true, true);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            return new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 30000 },
                    new HitCircle { StartTime = 35000 },
                },
            };
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new TestWorkingBeatmap(beatmap, storyboard, Audio);
    }
}
