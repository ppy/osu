// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckInconsistentTimingControlPointsTest
    {
        private CheckInconsistentTimingControlPoints check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckInconsistentTimingControlPoints();
        }

        [Test]
        public void TestConsistentTiming()
        {
            var beatmaps = createBeatmapSetWithTiming(
                new[] { 1000.0, 2000.0 }, // Timing at 1000ms and 2000ms
                new[] { 1000.0, 2000.0 } // Same timing
            );

            var context = createContext(beatmaps[0], beatmaps);
            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestMissingTimingPoint()
        {
            var beatmaps = createBeatmapSetWithTiming(
                new[] { 1000.0, 2000.0 }, // Reference has timing at 1000ms and 2000ms
                new[] { 1000.0 } // Second difficulty missing timing at 2000ms
            );

            var context = createContext(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.All(issue => issue.Template is CheckInconsistentTimingControlPoints.IssueTemplateMissingTimingPoint));
        }

        [Test]
        public void TestInconsistentBPM()
        {
            var beatmaps = createBeatmapSetWithBPM(
                new[] { (1000.0, 500.0) }, // Reference: 120 BPM (500ms beat length)
                new[] { (1000.0, 600.0) } // Second: 100 BPM (600ms beat length)
            );

            var context = createContext(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.All(issue => issue.Template is CheckInconsistentTimingControlPoints.IssueTemplateInconsistentBPM));
        }

        [Test]
        public void TestInconsistentMeter()
        {
            var beatmaps = createBeatmapSetWithMeter(
                new[] { (1000.0, TimeSignature.SimpleQuadruple) }, // Reference: 4/4
                new[] { (1000.0, TimeSignature.SimpleTriple) } // Second: 3/4
            );

            var context = createContext(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.All(issue => issue.Template is CheckInconsistentTimingControlPoints.IssueTemplateInconsistentMeter));
        }

        [Test]
        public void TestDecimalOffset()
        {
            var beatmaps = createBeatmapSetWithTiming(
                new[] { 1000.0 }, // Reference at exactly 1000ms
                new[] { 1000.5 } // Second at 1000.5ms (decimal difference)
            );

            var context = createContext(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.All(issue => issue.Template is CheckInconsistentTimingControlPoints.IssueTemplateMissingTimingPointMinor));
        }

        [Test]
        public void TestSingleDifficulty()
        {
            var beatmaps = createBeatmapSetWithTiming(
                new[] { 1000.0, 2000.0 } // Only one difficulty
            );

            var context = createContext(beatmaps[0], beatmaps);
            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestExtraTimingPoint()
        {
            var beatmaps = createBeatmapSetWithTiming(
                new[] { 1000.0 }, // Reference has timing at 1000ms
                new[] { 1000.0, 2000.0 } // Second has additional timing at 2000ms
            );

            var context = createContext(beatmaps[0], beatmaps);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.All(issue => issue.Template is CheckInconsistentTimingControlPoints.IssueTemplateExtraTimingPoint));
        }

        private IBeatmap[] createBeatmapSetWithTiming(params double[][] timingPoints)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[timingPoints.Length];

            for (int i = 0; i < timingPoints.Length; i++)
            {
                beatmaps[i] = createBeatmapWithTiming(timingPoints[i], $"Difficulty {i + 1}");
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
            }

            foreach (var beatmap in beatmaps)
                beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);

            return beatmaps;
        }

        private IBeatmap[] createBeatmapSetWithBPM(params (double time, double beatLength)[][] timingData)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[timingData.Length];

            for (int i = 0; i < timingData.Length; i++)
            {
                beatmaps[i] = createBeatmapWithBPM(timingData[i], $"Difficulty {i + 1}");
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
            }

            foreach (var beatmap in beatmaps)
                beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);

            return beatmaps;
        }

        private IBeatmap[] createBeatmapSetWithMeter(params (double time, TimeSignature meter)[][] timingData)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[timingData.Length];

            for (int i = 0; i < timingData.Length; i++)
            {
                beatmaps[i] = createBeatmapWithMeter(timingData[i], $"Difficulty {i + 1}");
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
            }

            foreach (var beatmap in beatmaps)
                beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);

            return beatmaps;
        }

        private IBeatmap createBeatmapWithTiming(double[] timingPoints, string difficultyName)
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Metadata = new BeatmapMetadata()
                },
                ControlPointInfo = new ControlPointInfo()
            };

            foreach (double time in timingPoints)
            {
                beatmap.ControlPointInfo.Add(time, new TimingControlPoint
                {
                    BeatLength = 500 // 120 BPM
                });
            }

            return beatmap;
        }

        private IBeatmap createBeatmapWithBPM((double time, double beatLength)[] timingData, string difficultyName)
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Metadata = new BeatmapMetadata()
                },
                ControlPointInfo = new ControlPointInfo()
            };

            foreach ((double time, double beatLength) in timingData)
            {
                beatmap.ControlPointInfo.Add(time, new TimingControlPoint
                {
                    BeatLength = beatLength
                });
            }

            return beatmap;
        }

        private IBeatmap createBeatmapWithMeter((double time, TimeSignature meter)[] timingData, string difficultyName)
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Metadata = new BeatmapMetadata()
                },
                ControlPointInfo = new ControlPointInfo()
            };

            foreach ((double time, var meter) in timingData)
            {
                beatmap.ControlPointInfo.Add(time, new TimingControlPoint
                {
                    BeatLength = 500, // 120 BPM
                    TimeSignature = meter
                });
            }

            return beatmap;
        }

        private BeatmapVerifierContext createContext(IBeatmap currentBeatmap, IBeatmap[] allDifficulties)
        {
            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap), currentBeatmap);
            var verifiedOtherBeatmaps = allDifficulties.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, DifficultyRating.ExpertPlus);
        }
    }
}
