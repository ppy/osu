// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Edit.Checks;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckTaikoInconsistentSkipBarLineTest
    {
        private CheckTaikoInconsistentSkipBarLine check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckTaikoInconsistentSkipBarLine();
        }

        [Test]
        public void TestConsistentOmitFirstBarLine()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) }, // Reference
                new[] { (1000.0, false), (2000.0, true) } // Same settings
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestInconsistentOmitFirstBarLine()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) }, // Reference
                new[] { (1000.0, true), (2000.0, false) } // Different settings
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.All(issue => issue.Template is CheckTaikoInconsistentSkipBarLine.IssueTemplateInconsistentOmitFirstBarLine));
            Assert.That(issues[0].Time, Is.EqualTo(1000.0));
            Assert.That(issues[1].Time, Is.EqualTo(2000.0));
        }

        [Test]
        public void TestPartiallyInconsistentOmitFirstBarLine()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true), (3000.0, false) }, // Reference
                new[] { (1000.0, false), (2000.0, false), (3000.0, false) } // Only second differs
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckTaikoInconsistentSkipBarLine.IssueTemplateInconsistentOmitFirstBarLine);
            Assert.That(issues[0].Time, Is.EqualTo(2000.0));
        }

        [Test]
        public void TestSingleDifficulty()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) } // Only one difficulty
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestNonTaikoBeatmaps()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) }, // Reference
                new[] { (1000.0, true), (2000.0, false) } // Different settings
            );

            // Make both beatmaps non-taiko
            beatmaps[0].BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;
            beatmaps[1].BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestMixedRulesets()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) }, // Reference
                new[] { (1000.0, true), (2000.0, false) } // Different settings
            );

            // Make reference taiko, other non-taiko
            beatmaps[0].BeatmapInfo.Ruleset = new TaikoRuleset().RulesetInfo;
            beatmaps[1].BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestMissingTimingPoints()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) }, // Reference has 2 points
                new[] { (1000.0, false) } // Other has only 1 point (missing 2000.0)
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            // Should only check the existing timing point at 1000.0 (consistent, no issue)
            // The missing 2000.0 point should be ignored by this check
            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void TestExtraTimingPoints()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false) }, // Reference has 1 point
                new[] { (1000.0, false), (2000.0, true) } // Other has extra point
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            // Should only check the existing timing point at 1000.0 (consistent, no issue)
            // The extra 2000.0 point should be ignored by this check
            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void TestMultipleDifficultiesWithInconsistencies()
        {
            var beatmaps = createBeatmapSetWithTimingPoints(
                new[] { (1000.0, false), (2000.0, true) }, // Reference
                new[] { (1000.0, true), (2000.0, true) }, // First differs
                new[] { (1000.0, false), (2000.0, false) } // Second differs
            );

            var context = createContextWithMultipleDifficulties(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            // Should have issues for both other difficulties
            Assert.That(issues, Has.Count.EqualTo(2)); // 1000.0 from diff2, 2000.0 from diff3
            Assert.That(issues.All(issue => issue.Template is CheckTaikoInconsistentSkipBarLine.IssueTemplateInconsistentOmitFirstBarLine));
            Assert.That(issues[0].Time, Is.EqualTo(1000.0));
            Assert.That(issues[1].Time, Is.EqualTo(2000.0));
        }

        private IBeatmap[] createBeatmapSetWithTimingPoints(params (double time, bool omitFirstBarLine)[][] timingData)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[timingData.Length];

            for (int i = 0; i < timingData.Length; i++)
            {
                beatmaps[i] = createBeatmapWithTimingPoints(timingData[i], $"Difficulty {i + 1}");
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
                beatmaps[i].BeatmapInfo.Ruleset = new TaikoRuleset().RulesetInfo;
            }

            // Configure the beatmapset to contain all the beatmap infos
            foreach (var beatmap in beatmaps)
                beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);

            return beatmaps;
        }

        private IBeatmap createBeatmapWithTimingPoints((double time, bool omitFirstBarLine)[] timingData, string difficultyName)
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Metadata = new BeatmapMetadata()
                }
            };

            foreach ((double time, bool omitFirstBarLine) in timingData)
            {
                beatmap.ControlPointInfo.Add(time, new TimingControlPoint
                {
                    BeatLength = 500, // Standard BPM
                    OmitFirstBarLine = omitFirstBarLine
                });
            }

            return beatmap;
        }

        private BeatmapVerifierContext createContextWithMultipleDifficulties(IBeatmap currentBeatmap, IBeatmap[] allDifficulties)
        {
            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap), currentBeatmap);
            var verifiedOtherBeatmaps = allDifficulties.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, DifficultyRating.ExpertPlus);
        }
    }
}
