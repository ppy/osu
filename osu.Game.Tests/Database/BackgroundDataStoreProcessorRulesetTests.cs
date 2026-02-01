// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#pragma warning disable IDE0005
using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Database
{
    [HeadlessTest]
    public partial class BackgroundDataStoreProcessorRulesetTests : OsuTestScene, ILocalUserPlayInfo
    {
        public IBindable<LocalUserPlayingState> PlayingState => isPlaying;

        private readonly Bindable<LocalUserPlayingState> isPlaying = new Bindable<LocalUserPlayingState>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Set not playing", () => isPlaying.Value = LocalUserPlayingState.NotPlaying);
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

            AddStep("Add dummy beatmap", () => Realm.Write(r =>
            {
                if (!r.All<BeatmapInfo>().Any())
                {
                    r.Add(new BeatmapInfo
                    {
                        BeatmapSet = new BeatmapSetInfo(),
                        Ruleset = r.All<RulesetInfo>().First(),
                        Difficulty = new BeatmapDifficulty()
                    });
                }
            }));

            AddStep("Add score for unavailable ruleset", () => Realm.Write(r => r.Add(scoreInfo = new ScoreInfo(
                ruleset: rulesetInfo,
                beatmap: r.All<BeatmapInfo>().First())
            {
                TotalScoreVersion = 30000001
            })));

            AddStep("Run background processor", () => Add(processor = new TestBackgroundDataStoreProcessor()));
            AddWaitStep("Wait for potential download", 2000);
            AddUntilStep("Wait for completion", () => processor.Completed);

            AddAssert("Score not marked as failed", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.BackgroundReprocessingFailed), () => Is.False);
            AddAssert("Score version not upgraded", () => Realm.Run(r => r.Find<ScoreInfo>(scoreInfo.ID)!.TotalScoreVersion), () => Is.EqualTo(30000001));
        }

        public partial class TestBackgroundDataStoreProcessor : BackgroundDataStoreProcessor
        {
            protected override int TimeToSleepDuringGameplay => 10;

            public bool Completed => ProcessingTask.IsCompleted;
        }
    }
}
