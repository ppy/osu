// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneTopLocalRank : OsuTestScene
    {
        private RulesetStore rulesets;
        private BeatmapManager beatmapManager;
        private ScoreManager scoreManager;
        private TopLocalRank topLocalRank;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(scoreManager = new ScoreManager(rulesets, () => beatmapManager, LocalStorage, Realm, Scheduler, API));
            Dependencies.Cache(Realm);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
        }

        private BeatmapInfo importedBeatmap => beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First(b => b.Ruleset.ShortName == "osu");

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Delete all scores", () => scoreManager.Delete());

            AddStep("Create local rank", () =>
            {
                Add(topLocalRank = new TopLocalRank(importedBeatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(10),
                });
            });

            AddAssert("No rank displayed initially", () => topLocalRank.Rank == null);
        }

        [Test]
        public void TestBasicImportDelete()
        {
            ScoreInfo testScoreInfo = null;

            AddStep("Add score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("B rank displayed", () => topLocalRank.Rank == ScoreRank.B);

            AddStep("Delete score", () =>
            {
                scoreManager.Delete(testScoreInfo);
            });

            AddUntilStep("No rank displayed", () => topLocalRank.Rank == null);
        }

        [Test]
        public void TestRulesetChange()
        {
            ScoreInfo testScoreInfo;

            AddStep("Add score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("Wait for initial display", () => topLocalRank.Rank == ScoreRank.B);

            AddStep("Change ruleset", () => Ruleset.Value = rulesets.GetRuleset("fruits"));
            AddUntilStep("No rank displayed", () => topLocalRank.Rank == null);

            AddStep("Change ruleset back", () => Ruleset.Value = rulesets.GetRuleset("osu"));
            AddUntilStep("B rank displayed", () => topLocalRank.Rank == ScoreRank.B);
        }

        [Test]
        public void TestHigherScoreSet()
        {
            ScoreInfo testScoreInfo = null;

            AddStep("Add score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("B rank displayed", () => topLocalRank.Rank == ScoreRank.B);

            AddStep("Add higher score for current user", () =>
            {
                var testScoreInfo2 = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo2.User = API.LocalUser.Value;
                testScoreInfo2.Rank = ScoreRank.S;
                testScoreInfo2.TotalScore = testScoreInfo.TotalScore + 1;
                testScoreInfo2.Statistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Miss] = 0,
                    [HitResult.Perfect] = 970,
                    [HitResult.SmallTickHit] = 75,
                    [HitResult.LargeTickHit] = 150,
                    [HitResult.LargeBonus] = 10,
                    [HitResult.SmallBonus] = 50
                };

                scoreManager.Import(testScoreInfo2);
            });

            AddUntilStep("S rank displayed", () => topLocalRank.Rank == ScoreRank.S);
        }

        [Test]
        public void TestLegacyScore()
        {
            ScoreInfo testScoreInfo = null;

            AddStep("Add legacy score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;
                testScoreInfo.TotalScore = scoreManager.GetTotalScoreAsync(testScoreInfo, ScoringMode.Classic).GetResultSafely();

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("B rank displayed", () => topLocalRank.Rank == ScoreRank.B);

            AddStep("Add higher score for current user", () =>
            {
                var testScoreInfo2 = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo2.User = API.LocalUser.Value;
                testScoreInfo2.Rank = ScoreRank.S;
                testScoreInfo2.Statistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Miss] = 0,
                    [HitResult.Perfect] = 970,
                    [HitResult.SmallTickHit] = 75,
                    [HitResult.LargeTickHit] = 150,
                    [HitResult.LargeBonus] = 10,
                    [HitResult.SmallBonus] = 50
                };

                testScoreInfo2.TotalScore = scoreManager.GetTotalScoreAsync(testScoreInfo).GetResultSafely();

                // ensure standardised total score is less than classic, otherwise this test is pointless.
                Debug.Assert(testScoreInfo2.TotalScore < testScoreInfo.TotalScore);

                scoreManager.Import(testScoreInfo2);
            });

            AddUntilStep("S rank displayed", () => topLocalRank.Rank == ScoreRank.S);
        }
    }
}
