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
    public class TestSceneTopLocalRank : OsuTestScene
    {
        private RulesetStore rulesets;
        private BeatmapManager beatmapManager;
        private ScoreManager scoreManager;
        private TopLocalRank topLocalRank;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(scoreManager = new ScoreManager(rulesets, () => beatmapManager, LocalStorage, ContextFactory, Scheduler));
            Dependencies.Cache(ContextFactory);

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
        }

        [Test]
        public void TestBasicImportDelete()
        {
            ScoreInfo testScoreInfo = null;

            AddAssert("Initially not present", () => !topLocalRank.IsPresent);

            AddStep("Add score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("Became present", () => topLocalRank.IsPresent);
            AddAssert("Correct rank", () => topLocalRank.Rank == ScoreRank.B);

            AddStep("Delete score", () =>
            {
                scoreManager.Delete(testScoreInfo);
            });

            AddUntilStep("Became not present", () => !topLocalRank.IsPresent);
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

            AddUntilStep("Wait for initial presence", () => topLocalRank.IsPresent);

            AddStep("Change ruleset", () => Ruleset.Value = rulesets.GetRuleset("fruits"));
            AddUntilStep("Became not present", () => !topLocalRank.IsPresent);

            AddStep("Change ruleset back", () => Ruleset.Value = rulesets.GetRuleset("osu"));
            AddUntilStep("Became present", () => topLocalRank.IsPresent);
        }

        [Test]
        public void TestHigherScoreSet()
        {
            ScoreInfo testScoreInfo = null;

            AddAssert("Initially not present", () => !topLocalRank.IsPresent);

            AddStep("Add score for current user", () =>
            {
                testScoreInfo = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo.User = API.LocalUser.Value;
                testScoreInfo.Rank = ScoreRank.B;

                scoreManager.Import(testScoreInfo);
            });

            AddUntilStep("Became present", () => topLocalRank.IsPresent);
            AddAssert("Correct rank", () => topLocalRank.Rank == ScoreRank.B);

            AddStep("Add higher score for current user", () =>
            {
                var testScoreInfo2 = TestResources.CreateTestScoreInfo(importedBeatmap);

                testScoreInfo2.User = API.LocalUser.Value;
                testScoreInfo2.Rank = ScoreRank.S;
                testScoreInfo2.TotalScore = testScoreInfo.TotalScore + 1;

                scoreManager.Import(testScoreInfo2);
            });

            AddAssert("Correct rank", () => topLocalRank.Rank == ScoreRank.S);
        }
    }
}
