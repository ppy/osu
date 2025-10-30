// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneReplayPlayer : RateAdjustedBeatmapTestScene
    {
        protected TestReplayPlayer Player = null!;

        [Test]
        public void TestFailedBeatmapLoad()
        {
            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo, withHitObjects: false));

            AddUntilStep("wait for exit", () => Player.IsCurrentScreen());
        }

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
                AudioLeadIn = 60000
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
        public void TestReplayDoesNotFailUntilRunningOutOfFrames()
        {
            var score = new Score
            {
                ScoreInfo = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo),
                Replay = new Replay
                {
                    Frames =
                    {
                        new OsuReplayFrame(0, Vector2.Zero),
                        new OsuReplayFrame(10000, Vector2.Zero),
                    }
                }
            };
            score.ScoreInfo.Mods = [];
            score.ScoreInfo.Rank = ScoreRank.F;
            AddStep("set global state", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                Ruleset.Value = Beatmap.Value.BeatmapInfo.Ruleset;
                SelectedMods.Value = score.ScoreInfo.Mods;
            });
            AddStep("create player", () => Player = new TestReplayPlayer(score, showResults: false));
            AddStep("load player", () => LoadScreen(Player));
            AddUntilStep("wait for loaded", () => Player.IsCurrentScreen());
            AddStep("seek to 8000", () => Player.Seek(8000));
            AddUntilStep("fail indicator visible", () => Player.ChildrenOfType<ReplayFailIndicator>().Any(indicator => indicator.IsAlive && indicator.IsPresent));
        }

        [Test]
        public void TestPlayerLoaderSettingsHover()
        {
            loadPlayerWithBeatmap();

            AddUntilStep("wait for settings overlay hidden", () => settingsOverlay().Expanded.Value, () => Is.False);
            AddStep("move mouse to right of screen", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopRight));
            AddUntilStep("wait for settings overlay visible", () => settingsOverlay().Expanded.Value, () => Is.True);
            AddStep("move mouse to centre of screen", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for settings overlay hidden", () => settingsOverlay().Expanded.Value, () => Is.False);

            PlayerSettingsOverlay settingsOverlay() => Player.ChildrenOfType<PlayerSettingsOverlay>().Single();
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
