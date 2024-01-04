// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneReplayPlayer : RateAdjustedBeatmapTestScene
    {
        protected TestReplayPlayer Player = null!;

        [Test]
        public void TestPauseViaSpace()
        {
            loadPlayerWithBeatmap();

            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Pause playback with space", () => InputManager.Key(Key.Space));

            AddAssert("player not exited", () => Player.IsCurrentScreen());

            AddUntilStep("Time stopped progressing", () =>
            {
                double current = Player.GameplayClockContainer.CurrentTime;
                bool changed = lastTime != current;
                lastTime = current;

                return !changed;
            });

            AddWaitStep("wait some", 10);

            AddAssert("Time still stopped", () => lastTime == Player.GameplayClockContainer.CurrentTime);
        }

        [Test]
        public void TestDoesNotFailOnExit()
        {
            loadPlayerWithBeatmap();

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);
            AddAssert("ensure rank is not fail", () => Player.ScoreProcessor.Rank.Value, () => Is.Not.EqualTo(ScoreRank.F));
            AddStep("exit player", () => Player.Exit());
            AddUntilStep("wait for exit", () => Player.Parent == null);
            AddAssert("ensure rank is not fail", () => Player.ScoreProcessor.Rank.Value, () => Is.Not.EqualTo(ScoreRank.F));
        }

        [Test]
        public void TestPauseViaSpaceWithSkip()
        {
            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo = { AudioLeadIn = 60000 }
            });

            AddUntilStep("wait for skip overlay", () => Player.ChildrenOfType<SkipOverlay>().First().IsButtonVisible);

            AddStep("Skip with space", () => InputManager.Key(Key.Space));

            AddAssert("Player not paused", () => !Player.DrawableRuleset.IsPaused.Value);

            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Pause playback with space", () => InputManager.Key(Key.Space));

            AddAssert("player not exited", () => Player.IsCurrentScreen());

            AddUntilStep("Time stopped progressing", () =>
            {
                double current = Player.GameplayClockContainer.CurrentTime;
                bool changed = lastTime != current;
                lastTime = current;

                return !changed;
            });

            AddWaitStep("wait some", 10);

            AddAssert("Time still stopped", () => lastTime == Player.GameplayClockContainer.CurrentTime);
        }

        [Test]
        public void TestPauseViaMiddleMouse()
        {
            loadPlayerWithBeatmap();

            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Pause playback with middle mouse", () => InputManager.Click(MouseButton.Middle));

            AddAssert("player not exited", () => Player.IsCurrentScreen());

            AddUntilStep("Time stopped progressing", () =>
            {
                double current = Player.GameplayClockContainer.CurrentTime;
                bool changed = lastTime != current;
                lastTime = current;

                return !changed;
            });

            AddWaitStep("wait some", 10);

            AddAssert("Time still stopped", () => lastTime == Player.GameplayClockContainer.CurrentTime);
        }

        [Test]
        public void TestSeekBackwards()
        {
            loadPlayerWithBeatmap();

            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Seek backwards", () =>
            {
                lastTime = Player.GameplayClockContainer.CurrentTime;
                InputManager.Key(Key.Left);
            });

            AddAssert("Jumped backwards", () => Player.GameplayClockContainer.CurrentTime - lastTime < 0);
        }

        [Test]
        public void TestSeekForwards()
        {
            loadPlayerWithBeatmap();

            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Seek forwards", () =>
            {
                lastTime = Player.GameplayClockContainer.CurrentTime;
                InputManager.Key(Key.Right);
            });

            AddAssert("Jumped forwards", () => Player.GameplayClockContainer.CurrentTime - lastTime > 500);
        }

        [Test]
        public void TestNotRemovingRedundantMods()
        {
            var redundantMod = new OsuModDifficultyAdjust();

            AddStep("create player with redundant mod", () =>
            {
                var ruleset = new OsuRuleset();
                Beatmap.Value = CreateWorkingBeatmap(ruleset.RulesetInfo);
                SelectedMods.Value = new Mod[] { redundantMod, new OsuModAutoplay() };
                Player = new TestReplayPlayer(false);
            });

            AddStep("load player", () => LoadScreen(Player));

            AddAssert("redundant mod was not removed", () => Player.Mods.Value.Contains(redundantMod));
        }

        private void loadPlayerWithBeatmap(IBeatmap? beatmap = null)
        {
            AddStep("create player", () =>
            {
                CreatePlayer(new OsuRuleset(), beatmap);
            });

            AddStep("Load player", () => LoadScreen(Player));
            AddUntilStep("player loaded", () => Player.IsLoaded);
        }

        protected void CreatePlayer(Ruleset ruleset, IBeatmap? beatmap = null)
        {
            Beatmap.Value = beatmap != null
                ? CreateWorkingBeatmap(beatmap)
                : CreateWorkingBeatmap(ruleset.RulesetInfo);

            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };

            Player = new TestReplayPlayer(false);
        }
    }
}
