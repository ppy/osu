// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using System;
using System.Threading.Tasks;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneReplayPlayerLoader : OsuTestScene
    {
        private TestReplayPlayerLoader playerLoader;
        private OsuScreenStack stack;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = stack = new OsuScreenStack { RelativeSizeAxes = Axes.Both };
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            playerLoader = null;
        });

        [Test]
        public void TestWaitForDownload()
        {
            ScoreInfo score = new ScoreInfo
            {
                Ruleset = Ruleset.Value,
                OnlineScoreID = -66,
            };

            TestPlayer player = null;

            AddStep("create new loader", () => stack.Push(playerLoader = new TestReplayPlayerLoader(score, _ => player = new TestPlayer(false, false))));
            AddUntilStep("wait for current screen", () => playerLoader.IsCurrentScreen());
            AddAssert("player is still null", () => player == null);
            AddAssert("download request created", () => playerLoader.DownloadRequest != null);
            AddStep("trigger download success", () => playerLoader.TriggerDownloadSuccess());
            AddAssert("player has been created", () => player != null);
            AddStep("remove player reference", () => player = null);
        }

        private class TestReplayPlayerLoader : ReplayPlayerLoader
        {
            private readonly ScoreInfo score;

            public DownloadReplayRequest DownloadRequest;

            public TestReplayPlayerLoader(ScoreInfo score, Func<Score, Player> createPlayer)
                : base(score, createPlayer)
            {
                this.score = score;
            }

            private Action<Player> onPlayerLoad;

            protected override Task CreatePlayerLoadTask(Action<Player> onLoad)
            {
                onPlayerLoad = onLoad;

                return Task.Run(() =>
                {
                    DownloadRequest = new DownloadReplayRequest(score);
                });
            }

            public void TriggerDownloadSuccess()
            {
                LoadReplay(string.Empty, onPlayerLoad);
            }

            protected override Score CreateReplayScore(string _)
            {
                return Ruleset.Value.CreateInstance().GetAutoplayMod().CreateReplayScore(Beatmap.Value.GetPlayableBeatmap(Ruleset.Value, Array.Empty<Mod>()));
            }
        }
    }
}
