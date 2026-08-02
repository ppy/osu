// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckInconsistentAudioTest
    {
        private CheckInconsistentAudio check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckInconsistentAudio();
        }

        [Test]
        public void TestConsistentAudio()
        {
            var beatmaps = createBeatmapSetWithAudio("audio.mp3", "audio.mp3");
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestInconsistentAudio()
        {
            var beatmaps = createBeatmapSetWithAudio("audio1.mp3", "audio2.mp3");
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckInconsistentAudio.IssueTemplateInconsistentAudio);
            Assert.That(issues.Single().ToString(), Contains.Substring("audio1.mp3"));
            Assert.That(issues.Single().ToString(), Contains.Substring("audio2.mp3"));
        }

        [Test]
        public void TestInconsistentAudioWithNull()
        {
            var beatmaps = createBeatmapSetWithAudio("audio.mp3", null);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckInconsistentAudio.IssueTemplateInconsistentAudio);
            Assert.That(issues.Single().ToString(), Contains.Substring("audio.mp3"));
            Assert.That(issues.Single().ToString(), Contains.Substring("not set"));
        }

        [Test]
        public void TestInconsistentAudioWithEmptyString()
        {
            var beatmaps = createBeatmapSetWithAudio("audio.mp3", "");
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckInconsistentAudio.IssueTemplateInconsistentAudio);
            Assert.That(issues.Single().ToString(), Contains.Substring("audio.mp3"));
            Assert.That(issues.Single().ToString(), Contains.Substring("not set"));
        }

        [Test]
        public void TestBothAudioNotSet()
        {
            var beatmaps = createBeatmapSetWithAudio("", "");
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestMultipleInconsistencies()
        {
            var beatmaps = createBeatmapSetWithAudio("audio1.mp3", "audio2.mp3", "audio3.mp3");
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.All(issue => issue.Template is CheckInconsistentAudio.IssueTemplateInconsistentAudio));
        }

        [Test]
        public void TestSingleDifficulty()
        {
            var beatmaps = createBeatmapSetWithAudio("audio.mp3");
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        private IBeatmap createBeatmapWithAudio(string audioFile, RealmNamedFileUsage? file)
        {
            var beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { AudioFile = audioFile },
                    BeatmapSet = new BeatmapSetInfo()
                }
            };

            if (file != null)
                beatmap.BeatmapInfo.BeatmapSet!.Files.Add(file);

            return beatmap;
        }

        private IBeatmap[] createBeatmapSetWithAudio(params string?[] audioFiles)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[audioFiles.Length];

            for (int i = 0; i < audioFiles.Length; i++)
            {
                string? audioFile = audioFiles[i];
                var file = !string.IsNullOrEmpty(audioFile) ? CheckTestHelpers.CreateMockFile("mp3") : null;

                beatmaps[i] = createBeatmapWithAudio(audioFile ?? "", file);
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
                beatmaps[i].BeatmapInfo.DifficultyName = $"Difficulty {i + 1}";
                beatmapSet.Beatmaps.Add(beatmaps[i].BeatmapInfo);
            }

            return beatmaps;
        }

        private BeatmapVerifierContext createContextWithMultipleDifficulties(IBeatmap currentBeatmap, IBeatmap[] allDifficulties)
        {
            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap), currentBeatmap);
            var verifiedOtherBeatmaps = allDifficulties.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, DifficultyRating.ExpertPlus);
        }
    }
}
