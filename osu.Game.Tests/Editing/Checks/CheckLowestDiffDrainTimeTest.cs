// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckLowestDiffDrainTimeTest
    {
        private TestCheckLowestDiffDrainTime check = null!;

        [SetUp]
        public void Setup()
        {
            check = new TestCheckLowestDiffDrainTime();
        }

        [Test]
        public void TestSingleDifficultyMeetsRequirement()
        {
            var beatmap = createBeatmapWithDrainTime(4 * 60 * 1000, 3.5, "Hard"); // 4 minutes
            assertOk(beatmap);
        }

        [Test]
        public void TestSingleDifficultyTooShort()
        {
            var beatmap = createBeatmapWithDrainTime(2 * 60 * 1000, 3.5, "Hard"); // 2 minutes - too short for Hard
            assertTooShort(beatmap);
        }

        [Test]
        public void TestHardDifficultyAtThreshold()
        {
            var beatmap = createBeatmapWithDrainTime((3 * 60 + 30) * 1000, 3.5, "Hard"); // Exactly 3:30
            assertOk(beatmap);
        }

        [Test]
        public void TestHardDifficultyJustUnderThreshold()
        {
            var beatmap = createBeatmapWithDrainTime((3 * 60 + 29) * 1000, 3.5, "Hard"); // 3:29 - just under threshold
            assertTooShort(beatmap);
        }

        [Test]
        public void TestInsaneDifficultyAtThreshold()
        {
            var beatmap = createBeatmapWithDrainTime((4 * 60 + 15) * 1000, 4.5, "Insane"); // Exactly 4:15
            assertOk(beatmap);
        }

        [Test]
        public void TestInsaneDifficultyTooShort()
        {
            var beatmap = createBeatmapWithDrainTime(4 * 60 * 1000, 4.5, "Insane"); // 4:00 - too short for Insane
            assertTooShort(beatmap);
        }

        [Test]
        public void TestExpertDifficultyAtThreshold()
        {
            var beatmap = createBeatmapWithDrainTime(5 * 60 * 1000, 5.5, "Expert"); // Exactly 5:00
            assertOk(beatmap);
        }

        [Test]
        public void TestExpertDifficultyTooShort()
        {
            var beatmap = createBeatmapWithDrainTime((4 * 60 + 30) * 1000, 5.5, "Expert"); // 4:30 - too short for Expert
            assertTooShort(beatmap);
        }

        [Test]
        public void TestEasyDifficultyMeetsRequirement()
        {
            var beatmap = createBeatmapWithDrainTime(2 * 60 * 1000, 1.5, "Easy"); // 2 minutes - should be ok for Easy
            assertOk(beatmap);
        }

        [Test]
        public void TestNormalDifficultyMeetsRequirement()
        {
            var beatmap = createBeatmapWithDrainTime(2 * 60 * 1000, 2.5, "Normal"); // 2 minutes - should be ok for Normal
            assertOk(beatmap);
        }

        [Test]
        public void TestMultipleDifficultiesMeetsRequirement()
        {
            var difficulties = new List<IBeatmap>
            {
                createBeatmapWithDrainTime((3 * 60 + 30) * 1000, 3.5, "Hard"), // Hard - lowest difficulty, 3:30
                createBeatmapWithDrainTime((3 * 60 + 30) * 1000, 4.5, "Insane"),
                createBeatmapWithDrainTime((3 * 60 + 30) * 1000, 5.5, "Expert")
            };

            // All should be ok because lowest difficulty is Hard and drain time meets Hard requirement
            assertOkWithMultipleDifficulties(difficulties[0], difficulties);
            assertOkWithMultipleDifficulties(difficulties[1], difficulties);
            assertOkWithMultipleDifficulties(difficulties[2], difficulties);
        }

        [Test]
        public void TestMultipleDifficultiesTooShort()
        {
            var difficulties = new List<IBeatmap>
            {
                createBeatmapWithDrainTime(4 * 60 * 1000, 4.5, "Insane"), // Insane - lowest difficulty, 4:00
                createBeatmapWithDrainTime(4 * 60 * 1000, 5.5, "Expert") // Same drain time
            };

            // Should be too short because lowest difficulty is Insane and requires 4:15
            assertTooShortWithMultipleDifficulties(difficulties[0], difficulties);
            assertTooShortWithMultipleDifficulties(difficulties[1], difficulties);
        }

        [Test]
        public void TestPlayTimeVsDrainTimeNotHighestDifficulty()
        {
            var expertBeatmap = createBeatmapWithPlayTime(5 * 60 * 1000, 5.5, "Expert"); // 5:00 play time
            expertBeatmap.Breaks.Add(new BreakPeriod(60000, 100000)); // 40-second break

            var difficulties = new List<IBeatmap>
            {
                expertBeatmap, // Expert - 5:00 play, 4:20 drain
                createBeatmapWithPlayTime(5 * 60 * 1000, 6.5, "ExpertPlus") // ExpertPlus - highest difficulty
            };

            // The Expert difficulty (not highest) should use play time (5:00) and pass the Expert requirement
            assertOkWithMultipleDifficulties(difficulties[0], difficulties);
        }

        [Test]
        public void TestPlayTimeVsDrainTimeHighestDifficulty()
        {
            var expertBeatmap = createBeatmapWithPlayTime(5 * 60 * 1000, 5.5, "Expert"); // 5:00 play time
            expertBeatmap.Breaks.Add(new BreakPeriod(60000, 100000)); // 40-second break

            // As the highest difficulty with breaks > 30s, it should use drain time and fail
            assertTooShort(expertBeatmap);
        }

        private IBeatmap createBeatmapWithDrainTime(double drainTimeMs, double starRating = 3.5, string difficultyName = "Default")
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    StarRating = starRating,
                    DifficultyName = difficultyName,
                    Ruleset = new OsuRuleset().RulesetInfo
                },
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = 0 },
                    new HitObject { StartTime = drainTimeMs } // Last object at drain time
                }
            };

            return beatmap;
        }

        private IBeatmap createBeatmapWithPlayTime(double playTimeMs, double starRating = 3.5, string difficultyName = "Default")
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    StarRating = starRating,
                    DifficultyName = difficultyName,
                    Ruleset = new OsuRuleset().RulesetInfo
                },
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = 0 },
                    new HitObject { StartTime = playTimeMs } // Last object at play time
                }
            };

            return beatmap;
        }

        private void assertOk(IBeatmap beatmap)
        {
            var difficultyRating = StarDifficulty.GetDifficultyRating(beatmap.BeatmapInfo.StarRating);
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);

            Assert.That(check.Run(context), Is.Empty);
        }

        private void assertTooShort(IBeatmap beatmap)
        {
            var difficultyRating = StarDifficulty.GetDifficultyRating(beatmap.BeatmapInfo.StarRating);
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckLowestDiffDrainTime.IssueTemplateTooShort);
        }

        private void assertOkWithMultipleDifficulties(IBeatmap currentBeatmap, IEnumerable<IBeatmap> allDifficulties)
        {
            var context = createContextWithMultipleDifficulties(currentBeatmap, allDifficulties);

            Assert.That(check.Run(context), Is.Empty);
        }

        private void assertTooShortWithMultipleDifficulties(IBeatmap currentBeatmap, IEnumerable<IBeatmap> allDifficulties)
        {
            var context = createContextWithMultipleDifficulties(currentBeatmap, allDifficulties);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckLowestDiffDrainTime.IssueTemplateTooShort);
        }

        private BeatmapVerifierContext createContextWithMultipleDifficulties(IBeatmap currentBeatmap, IEnumerable<IBeatmap> allDifficulties)
        {
            var difficultiesArray = allDifficulties.ToArray();
            var currentDifficultyRating = StarDifficulty.GetDifficultyRating(currentBeatmap.BeatmapInfo.StarRating);

            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap), currentBeatmap);
            var verifiedOtherBeatmaps = difficultiesArray.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, currentDifficultyRating);
        }

        private class TestCheckLowestDiffDrainTime : CheckLowestDiffDrainTime
        {
            protected override IEnumerable<(DifficultyRating rating, double thresholdMs, string name)> GetThresholds()
            {
                // Same thresholds as `CheckOsuLowestDiffDrainTime` for testing
                yield return (DifficultyRating.Hard, new TimeSpan(0, 3, 30).TotalMilliseconds, "Hard");
                yield return (DifficultyRating.Insane, new TimeSpan(0, 4, 15).TotalMilliseconds, "Insane");
                yield return (DifficultyRating.Expert, new TimeSpan(0, 5, 0).TotalMilliseconds, "Expert");
            }
        }
    }
}
