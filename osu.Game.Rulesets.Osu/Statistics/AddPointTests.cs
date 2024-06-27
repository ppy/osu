// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Statistics
{
    [TestFixture]
    public partial class AddPointTests : OsuTestScene
    {
        private ScoreInfo score;
        private IBeatmap beatmap;
        private AccuracyHeatmap heatmap;

        [BackgroundDependencyLoader]
        private void load()
        {
            score = new ScoreInfo();
            beatmap = new TestBeatmap();
            Add(heatmap = new AccuracyHeatmap(score, beatmap));
        }

        [Test]
        public void TestAddPoint()
        {
            // Assuming the start, end, and hit points are within valid boundaries
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(10, 10);
            Vector2 hitPoint = new Vector2(5, 5);
            float radius = 10;

            // Act
            heatmap.AddPoint(start, end, hitPoint, radius);


            // Assert
            Assert.GreaterOrEqual(heatmap.PeakValue, 1); // PeakValue should have been updated

            // Additional assertions for pointGrid state if needed
            // Assert.AreEqual(expectedValue, heatmap.pointGrid.Content[r][c].Count);
        }

        [Test]
        public void TestAddPoint_OutsideBoundary()
        {
            // Assuming the hit point is outside valid boundaries
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(10, 10);
            Vector2 hitPoint = new Vector2(15, 15); // Outside the grid boundary
            float radius = 10;

            // Act
            heatmap.AddPoint(start, end, hitPoint, radius);

            // Assert
            Assert.AreEqual(1.0f, heatmap.PeakValue); // PeakValue should remain unchanged

            // Additional assertions for pointGrid state if needed
            // e.g., Assert.AreEqual(expectedValue, heatmap.pointGrid.Content[r][c].Count);
        }
    }

    // Mock IBeatmap implementation for testing purposes
    public class TestBeatmap : IBeatmap
    {
        public BeatmapInfo BeatmapInfo => throw new System.NotImplementedException();
        public BeatmapMetadata Metadata { get; }
        public BeatmapDifficulty Difficulty { get; set; }
        public ControlPointInfo ControlPointInfo { get; set; }
        public List<BreakPeriod> Breaks { get; }
        public List<string> UnhandledEventLines { get; }
        public double TotalBreakTime { get; }
        public IReadOnlyList<HitObject> HitObjects { get; }
        public IEnumerable<BeatmapStatistic> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        public double GetMostCommonBeatLength()
        {
            throw new System.NotImplementedException();
        }

        BeatmapInfo IBeatmap.BeatmapInfo { get; set; }
        public IBeatmap Clone() => throw new System.NotImplementedException();
        public double DistanceTo(Vector2 position) => throw new System.NotImplementedException();
        public double DifficultyAttribute { get; set; }
    }
}
