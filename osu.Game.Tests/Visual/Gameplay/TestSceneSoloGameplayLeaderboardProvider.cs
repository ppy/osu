// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneSoloGameplayLeaderboardProvider : OsuTestScene
    {
        [Test]
        public void TestLocalLeaderboardHasPositionsAutofilled()
        {
            SoloGameplayLeaderboardProvider provider = null!;

            var leaderboardManager = new LeaderboardManager();
            LoadComponent(leaderboardManager);
            var gameplayState = TestGameplayState.Create(new OsuRuleset());

            AddStep("fetch local", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(Beatmap.Value.BeatmapInfo, Ruleset.Value, BeatmapLeaderboardScope.Local, null)));
            AddStep("set scores", () =>
            {
                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(
                    Enumerable.Range(1, 100).Select(i => new ScoreInfo
                    {
                        TotalScore = 10_000 * (100 - i),
                        Position = i,
                    }).ToArray(),
                    scoresRequested: 100,
                    totalScores: 100,
                    null
                );
            });
            AddStep("create content", () => Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(LeaderboardManager), leaderboardManager),
                    (typeof(GameplayState), gameplayState)
                ],
                Children = new Drawable[]
                {
                    leaderboardManager,
                    provider = new SoloGameplayLeaderboardProvider()
                }
            });
            AddUntilStep("tracked score shows #101", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(101));
            AddUntilStep("tracked score ordered #101", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(101));
            AddStep("move score to #20", () => gameplayState.ScoreProcessor.TotalScore.Value = 802_000);
            AddUntilStep("tracked score shows #20", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(20));
            AddUntilStep("tracked score ordered #20", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(20));
            AddStep("move score to #1", () => gameplayState.ScoreProcessor.TotalScore.Value = 1_002_000);
            AddUntilStep("tracked score shows #1", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(1));
            AddUntilStep("tracked score ordered #1", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(1));
        }

        [Test]
        public void TestFullGlobalLeaderboard()
        {
            SoloGameplayLeaderboardProvider provider = null!;

            var leaderboardManager = new LeaderboardManager();
            LoadComponent(leaderboardManager);
            var gameplayState = TestGameplayState.Create(new OsuRuleset());

            AddStep("fetch local", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(Beatmap.Value.BeatmapInfo, Ruleset.Value, BeatmapLeaderboardScope.Global, null)));
            AddStep("set scores", () =>
            {
                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(
                    Enumerable.Range(1, 40).Select(i => new ScoreInfo
                    {
                        TotalScore = 600_000 + 10_000 * (40 - i),
                        Position = i,
                    }).ToArray(),
                    scoresRequested: 50,
                    totalScores: 40,
                    null
                );
            });
            AddStep("create content", () => Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(LeaderboardManager), leaderboardManager),
                    (typeof(GameplayState), gameplayState)
                ],
                Children = new Drawable[]
                {
                    leaderboardManager,
                    provider = new SoloGameplayLeaderboardProvider()
                }
            });
            AddUntilStep("tracked score shows #41", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(41));
            AddUntilStep("tracked score ordered #41", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(41));
            AddStep("move score to #20", () => gameplayState.ScoreProcessor.TotalScore.Value = 802_000);
            AddUntilStep("tracked score shows #20", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(20));
            AddUntilStep("tracked score ordered #20", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(20));
            AddStep("move score to #1", () => gameplayState.ScoreProcessor.TotalScore.Value = 1_002_000);
            AddUntilStep("tracked score shows #1", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(1));
            AddUntilStep("tracked score ordered #1", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(1));
        }

        [Test]
        public void TestPartialGlobalLeaderboard()
        {
            SoloGameplayLeaderboardProvider provider = null!;

            var leaderboardManager = new LeaderboardManager();
            LoadComponent(leaderboardManager);
            var gameplayState = TestGameplayState.Create(new OsuRuleset());

            AddStep("fetch local", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(Beatmap.Value.BeatmapInfo, Ruleset.Value, BeatmapLeaderboardScope.Global, null)));
            AddStep("set scores", () =>
            {
                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(
                    Enumerable.Range(1, 50).Select(i => new ScoreInfo
                    {
                        TotalScore = 500_000 + 10_000 * (50 - i),
                        Position = i
                    }).ToArray(),
                    scoresRequested: 50,
                    totalScores: 1337,
                    new ScoreInfo { TotalScore = 200_000 }
                );
            });
            AddStep("create content", () => Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(LeaderboardManager), leaderboardManager),
                    (typeof(GameplayState), gameplayState)
                ],
                Children = new Drawable[]
                {
                    leaderboardManager,
                    provider = new SoloGameplayLeaderboardProvider()
                }
            });
            AddUntilStep("tracked score shows no position", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.Null);
            AddUntilStep("tracked score ordered #52", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(52));
            AddStep("move score above user best", () => gameplayState.ScoreProcessor.TotalScore.Value = 202_000);
            AddUntilStep("tracked score shows no position", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.Null);
            AddUntilStep("tracked score ordered #51", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(51));
            AddStep("move score to #20", () => gameplayState.ScoreProcessor.TotalScore.Value = 802_000);
            AddUntilStep("tracked score shows #20", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(20));
            AddUntilStep("tracked score ordered #20", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(20));
            AddStep("move score to #1", () => gameplayState.ScoreProcessor.TotalScore.Value = 1_002_000);
            AddUntilStep("tracked score shows #1", () => provider.Scores.Single(s => s.Tracked).Position.Value, () => Is.EqualTo(1));
            AddUntilStep("tracked score ordered #1", () => provider.Scores.Single(s => s.Tracked).DisplayOrder.Value, () => Is.EqualTo(1));
        }
    }
}
