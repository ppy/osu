// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckVideoUsageTest
    {
        private CheckVideoUsage check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckVideoUsage();
        }

        [Test]
        public void TestConsistentVideoUsage()
        {
            var beatmap1 = createBeatmapWithVideo("Diff 1", "video.mp4", 1000);
            var beatmap2 = createBeatmapWithVideo("Diff 2", "video.mp4", 1000);

            var context = createContext(beatmap1, [beatmap2]);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestDifferentVideoFile()
        {
            var beatmap1 = createBeatmapWithVideo("Diff 1", "videoA.mp4", 0);
            var beatmap2 = createBeatmapWithVideo("Diff 2", "videoB.mp4", 500);

            var context = createContext(beatmap1, [beatmap2]);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckVideoUsage.IssueTemplateDifferentVideo);
        }

        [Test]
        public void TestDifferentStartTime()
        {
            var beatmap1 = createBeatmapWithVideo("Diff 1", "video.mp4", 0);
            var beatmap2 = createBeatmapWithVideo("Diff 2", "video.mp4", 500);

            var context = createContext(beatmap1, [beatmap2]);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckVideoUsage.IssueTemplateDifferentStartTime);
        }

        [Test]
        public void TestOtherDifficultyMissingVideo()
        {
            var beatmap1 = createBeatmapWithVideo("Diff 1", "video.mp4", 0);
            var beatmap2 = createBeatmapWithoutVideo("Diff 2");

            var context = createContext(beatmap1, [beatmap2]);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckVideoUsage.IssueTemplateMissingVideo);
        }

        [Test]
        public void TestCurrentDifficultyMissingVideo()
        {
            var beatmap1 = createBeatmapWithoutVideo("Diff 1");
            var beatmap2 = createBeatmapWithVideo("Diff 2", "video.mp4", 0);

            var context = createContext(beatmap1, [beatmap2]);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckVideoUsage.IssueTemplateMissingVideo);
        }

        [Test]
        public void TestBothDifficultiesMissingVideo()
        {
            var beatmap1 = createBeatmapWithoutVideo("Diff 1");
            var beatmap2 = createBeatmapWithoutVideo("Diff 2");

            var context = createContext(beatmap1, [beatmap2]);

            Assert.That(check.Run(context), Is.Empty);
        }

        private BeatmapVerifierContext.VerifiedBeatmap createBeatmapWithVideo(string difficultyName, string path, double startTime)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName
                }
            };

            var storyboard = new Storyboard();
            storyboard.GetLayer("Video").Add(new StoryboardVideo(path, startTime));

            var working = new TestWorkingBeatmap(beatmap, storyboard);
            return new BeatmapVerifierContext.VerifiedBeatmap(working, beatmap);
        }

        private BeatmapVerifierContext.VerifiedBeatmap createBeatmapWithoutVideo(string difficultyName)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName
                }
            };

            var storyboard = new Storyboard();
            // no video added
            var working = new TestWorkingBeatmap(beatmap, storyboard);
            return new BeatmapVerifierContext.VerifiedBeatmap(working, beatmap);
        }

        private BeatmapVerifierContext createContext(BeatmapVerifierContext.VerifiedBeatmap current, BeatmapVerifierContext.VerifiedBeatmap[] others)
        {
            return new BeatmapVerifierContext(
                current,
                others.ToList(),
                DifficultyRating.ExpertPlus
            );
        }
    }
}


