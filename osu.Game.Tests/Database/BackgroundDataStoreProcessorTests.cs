// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Database
{
    [HeadlessTest]
    public partial class BackgroundDataStoreProcessorTests : OsuTestScene, ILocalUserPlayInfo
    {
        public IBindable<bool> IsPlaying => isPlaying;

        private readonly Bindable<bool> isPlaying = new Bindable<bool>();

        private BeatmapSetInfo importedSet = null!;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu)
        {
            importedSet = BeatmapImportHelper.LoadQuickOszIntoOsu(osu).GetResultSafely();
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Set not playing", () => isPlaying.Value = false);
        }

        [Test]
        public void TestDifficultyProcessing()
        {
            AddAssert("Difficulty is initially set", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });

            AddStep("Reset difficulty", () =>
            {
                Realm.Write(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    foreach (var b in beatmapSetInfo.Beatmaps)
                        b.StarRating = -1;
                });
            });

            AddStep("Run background processor", () =>
            {
                Add(new TestBackgroundDataStoreProcessor());
            });

            AddUntilStep("wait for difficulties repopulated", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });
        }

        [Test]
        public void TestDifficultyProcessingWhilePlaying()
        {
            AddAssert("Difficulty is initially set", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });

            AddStep("Set playing", () => isPlaying.Value = true);

            AddStep("Reset difficulty", () =>
            {
                Realm.Write(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    foreach (var b in beatmapSetInfo.Beatmaps)
                        b.StarRating = -1;
                });
            });

            AddStep("Run background processor", () =>
            {
                Add(new TestBackgroundDataStoreProcessor());
            });

            AddWaitStep("wait some", 500);

            AddAssert("Difficulty still not populated", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating == -1);
                });
            });

            AddStep("Set not playing", () => isPlaying.Value = false);

            AddUntilStep("wait for difficulties repopulated", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });
        }

        [TestCase(30000001)]
        [TestCase(30000002)]
        [TestCase(30000003)]
        [TestCase(30000004)]
        [TestCase(30000005)]
        public void TestScoreUpgradeSuccess(int scoreVersion)
        {
            ScoreInfo scoreInfo = null!;

            AddStep("Add score which requires upgrade (and has beatmap)", () =>
            {
                Realm.Write(r =>
                {
                    r.Add(scoreInfo = new ScoreInfo(ruleset: r.All<RulesetInfo>().First(), beatmap: r.All<BeatmapInfo>().First())
                    {
                        TotalScoreVersion = scoreVersion,
                        LegacyTotalScore = 123456,
                        IsLegacyScore = true,
                    });
                });
            });

            AddStep("Run background processor", () => Add(new TestBackgroundDataStoreProcessor()));

            AddUntilStep("Score version upgraded", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.TotalScoreVersion), () => Is.EqualTo(LegacyScoreEncoder.LATEST_VERSION));
            AddAssert("Score not marked as failed", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.BackgroundReprocessingFailed), () => Is.False);
        }

        [Test]
        public void TestScoreUpgradeFailed()
        {
            ScoreInfo scoreInfo = null!;

            AddStep("Add score which requires upgrade (but has no beatmap)", () =>
            {
                Realm.Write(r =>
                {
                    r.Add(scoreInfo = new ScoreInfo(ruleset: r.All<RulesetInfo>().First(), beatmap: new BeatmapInfo
                    {
                        BeatmapSet = new BeatmapSetInfo(),
                        Ruleset = r.All<RulesetInfo>().First(),
                    })
                    {
                        TotalScoreVersion = 30000002,
                        IsLegacyScore = true,
                    });
                });
            });

            AddStep("Run background processor", () => Add(new TestBackgroundDataStoreProcessor()));

            AddUntilStep("Score marked as failed", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.BackgroundReprocessingFailed), () => Is.True);
            AddAssert("Score version not upgraded", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.TotalScoreVersion), () => Is.EqualTo(30000002));
        }

        [Test]
        public void TestCustomRulesetScoreNotSubjectToUpgrades([Values] bool available)
        {
            RulesetInfo rulesetInfo = null!;
            ScoreInfo scoreInfo = null!;
            TestBackgroundDataStoreProcessor processor = null!;

            AddStep("Add unavailable ruleset", () => Realm.Write(r => r.Add(rulesetInfo = new RulesetInfo
            {
                ShortName = Guid.NewGuid().ToString(),
                Available = available
            })));

            AddStep("Add score for unavailable ruleset", () => Realm.Write(r => r.Add(scoreInfo = new ScoreInfo(
                ruleset: rulesetInfo,
                beatmap: r.All<BeatmapInfo>().First())
            {
                TotalScoreVersion = 30000001
            })));

            AddStep("Run background processor", () => Add(processor = new TestBackgroundDataStoreProcessor()));
            AddUntilStep("Wait for completion", () => processor.Completed);

            AddAssert("Score not marked as failed", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.BackgroundReprocessingFailed), () => Is.False);
            AddAssert("Score version not upgraded", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.TotalScoreVersion), () => Is.EqualTo(30000001));
        }

        [Test]
        public void TestClassicModMultiplierChange()
        {
            AddStep("Add scores", () =>
            {
                string[] rulesets = ["osu", "taiko", "fruits", "mania"];

                foreach (string ruleset in rulesets)
                {
                    Realm.Write(r =>
                    {
                        r.Add(new ScoreInfo(ruleset: r.Find<RulesetInfo>(ruleset), beatmap: r.All<BeatmapInfo>().First())
                        {
                            TotalScoreVersion = 30000016,
                            TotalScore = 960_000,
                            APIMods = [new APIMod { Acronym = "CL" }]
                        });
                    });
                }
            });

            AddStep("Run background processor", () => Add(new TestBackgroundDataStoreProcessor()));

            AddUntilStep("Three scores updated", () => Realm.Run(r => r.All<ScoreInfo>().Count(score => score.TotalScore == 1_000_000)), () => Is.EqualTo(3));
            AddUntilStep("osu! score preserved", () => Realm.Run(r => r.All<ScoreInfo>().Count(score => score.TotalScore == 960_000)), () => Is.EqualTo(1));
            AddAssert("No fails", () => Realm.Run(r => r.All<ScoreInfo>().Count(score => score.BackgroundReprocessingFailed)), () => Is.Zero);
            AddAssert("All score versions upgraded", () => Realm.Run(r => r.All<ScoreInfo>().Count(score => score.TotalScoreVersion >= 30000017)), () => Is.EqualTo(4));
        }

        public partial class TestBackgroundDataStoreProcessor : BackgroundDataStoreProcessor
        {
            protected override int TimeToSleepDuringGameplay => 10;

            public bool Completed => ProcessingTask.IsCompleted;
        }
    }
}
