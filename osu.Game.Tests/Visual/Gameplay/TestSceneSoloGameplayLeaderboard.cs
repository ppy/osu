// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSoloGameplayLeaderboard : OsuTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private readonly ScoreProcessor scoreProcessor = TestGameplayState.Create(new OsuRuleset()).ScoreProcessor;

        private readonly BindableList<ScoreInfo> scores = new BindableList<ScoreInfo>();

        private readonly Bindable<bool> configVisibility = new Bindable<bool>();
        private readonly Bindable<PlayBeatmapDetailArea.TabType> beatmapTabType = new Bindable<PlayBeatmapDetailArea.TabType>();

        private SoloGameplayLeaderboard leaderboard = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.GameplayLeaderboard, configVisibility);
            config.BindWith(OsuSetting.BeatmapDetailTab, beatmapTabType);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear scores", () => scores.Clear());

            AddStep("create component", () =>
            {
                var trackingUser = new APIUser
                {
                    Username = "local user",
                    Id = 2,
                };

                Child = leaderboard = new SoloGameplayLeaderboard(trackingUser)
                {
                    Scores = { BindTarget = scores },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AlwaysVisible = { Value = false },
                    Expanded = { Value = true },
                };
            });

            AddStep("add scores", () => scores.AddRange(createSampleScores()));
        }

        [Test]
        public void TestLocalUser()
        {
            AddSliderStep("score", 0, 1000000, 500000, v => scoreProcessor.TotalScore.Value = v);
            AddSliderStep("accuracy", 0f, 1f, 0.5f, v => scoreProcessor.Accuracy.Value = v);
            AddSliderStep("combo", 0, 10000, 0, v => scoreProcessor.HighestCombo.Value = v);
            AddStep("toggle expanded", () => leaderboard.Expanded.Value = !leaderboard.Expanded.Value);
        }

        [TestCase(PlayBeatmapDetailArea.TabType.Local, 51)]
        [TestCase(PlayBeatmapDetailArea.TabType.Global, null)]
        [TestCase(PlayBeatmapDetailArea.TabType.Country, null)]
        [TestCase(PlayBeatmapDetailArea.TabType.Friends, null)]
        public void TestTrackedScorePosition(PlayBeatmapDetailArea.TabType tabType, int? expectedOverflowIndex)
        {
            AddStep($"change TabType to {tabType}", () => beatmapTabType.Value = tabType);
            AddUntilStep("tracked player is #50", () => leaderboard.TrackedScore?.ScorePosition, () => Is.EqualTo(50));

            AddStep("add one more score", () => scores.Add(new ScoreInfo { User = new APIUser { Username = "New player 1" }, TotalScore = RNG.Next(600000, 1000000) }));

            AddUntilStep("wait for sort", () => leaderboard.ChildrenOfType<GameplayLeaderboardScore>().First().ScorePosition != null);

            if (expectedOverflowIndex == null)
                AddUntilStep("tracked player has null position", () => leaderboard.TrackedScore?.ScorePosition, () => Is.Null);
            else
                AddUntilStep($"tracked player is #{expectedOverflowIndex}", () => leaderboard.TrackedScore?.ScorePosition, () => Is.EqualTo(expectedOverflowIndex));
        }

        [Test]
        public void TestVisibility()
        {
            AddStep("set config visible true", () => configVisibility.Value = true);
            AddUntilStep("leaderboard visible", () => leaderboard.Alpha == 1);

            AddStep("set config visible false", () => configVisibility.Value = false);
            AddUntilStep("leaderboard not visible", () => leaderboard.Alpha == 0);

            AddStep("set always visible", () => leaderboard.AlwaysVisible.Value = true);
            AddUntilStep("leaderboard visible", () => leaderboard.Alpha == 1);

            AddStep("set config visible true", () => configVisibility.Value = true);
            AddAssert("leaderboard still visible", () => leaderboard.Alpha == 1);
        }

        private static List<ScoreInfo> createSampleScores()
        {
            return new[]
            {
                new ScoreInfo { User = new APIUser { Username = @"peppy" }, TotalScore = RNG.Next(500000, 1000000) },
                new ScoreInfo { User = new APIUser { Username = @"smoogipoo" }, TotalScore = RNG.Next(500000, 1000000) },
                new ScoreInfo { User = new APIUser { Username = @"spaceman_atlas" }, TotalScore = RNG.Next(500000, 1000000) },
                new ScoreInfo { User = new APIUser { Username = @"frenzibyte" }, TotalScore = RNG.Next(500000, 1000000) },
                new ScoreInfo { User = new APIUser { Username = @"Susko3" }, TotalScore = RNG.Next(500000, 1000000) },
            }.Concat(Enumerable.Range(0, 44).Select(i => new ScoreInfo { User = new APIUser { Username = $"User {i + 1}" }, TotalScore = 1000000 - i * 10000 })).ToList();
        }
    }
}
