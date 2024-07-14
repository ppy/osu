// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Scoring;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneTopLocalRank : OsuTestScene
    {
        private RulesetStore rulesets = null!;
        private BeatmapManager beatmapManager = null!;
        private ScoreManager scoreManager = null!;
        private TopLocalRank topLocalRank = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(scoreManager = new ScoreManager(rulesets, () => beatmapManager, LocalStorage, Realm, API));
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
                Child = topLocalRank = new TopLocalRank(importedBeatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(10),
                };
            });

            AddAssert("No rank displayed initially", () => topLocalRank.DisplayedRank == null);
        }

        [Test]
        public void TestBasicImportDelete()
        {
            ScoreInfo testScoreInfo = null!;

            AddStep("Add score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("B rank displayed", () => topLocalRank.DisplayedRank == ScoreRank.B);

            AddStep("Delete score", () => scoreManager.Delete(testScoreInfo));

            AddUntilStep("No rank displayed", () => topLocalRank.DisplayedRank == null);
        }

        [Test]
        public void TestRulesetChange()
        {
            AddStep("Add score for current user", () =>
            {
                var testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("Wait for initial display", () => topLocalRank.DisplayedRank == ScoreRank.B);

            AddStep("Change ruleset", () => Ruleset.Value = rulesets.GetRuleset("fruits"));
            AddUntilStep("No rank displayed", () => topLocalRank.DisplayedRank == null);

            AddStep("Change ruleset back", () => Ruleset.Value = rulesets.GetRuleset("osu"));
            AddUntilStep("B rank displayed", () => topLocalRank.DisplayedRank == ScoreRank.B);
        }

        [Test]
        public void TestHigherScoreSet()
        {
            AddStep("Add score for current user", () =>
            {
                var testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("B rank displayed", () => topLocalRank.DisplayedRank == ScoreRank.B);

            AddStep("Add higher score for current user", () =>
            {
                var testScoreInfo2 = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo2.User = API.LocalUser.Value;
                testScoreInfo2.Rank = ScoreRank.X;
                testScoreInfo2.TotalScore = 1000000;
                testScoreInfo2.Statistics = testScoreInfo2.MaximumStatistics;

                scoreManager.Import(testScoreInfo2);
            });

            AddUntilStep("SS rank displayed", () => topLocalRank.DisplayedRank == ScoreRank.X);
        }

        [Test]
        public void TestLegacyScore()
        {
            ScoreInfo testScoreInfo = null!;

            AddStep("Add legacy score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("B rank displayed", () => topLocalRank.DisplayedRank == ScoreRank.B);

            AddStep("Add higher-graded score for current user", () =>
            {
                var testScoreInfo2 = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo2.User = API.LocalUser.Value;
                testScoreInfo2.Rank = ScoreRank.X;
                testScoreInfo2.Statistics = testScoreInfo2.MaximumStatistics;
                testScoreInfo2.TotalScore = testScoreInfo.TotalScore + 1;

                scoreManager.Import(testScoreInfo2);
            });

            AddUntilStep("SS rank displayed", () => topLocalRank.DisplayedRank == ScoreRank.X);
        }
    }
}
