// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckInconsistentMetadataTest
    {
        private CheckInconsistentMetadata check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckInconsistentMetadata();
        }

        [Test]
        public void TestConsistentMetadata()
        {
            var metadata = createMetadata("Test Artist", "Test Title", "Test Source", "Test Creator", "tag1 tag2");
            var beatmaps = createBeatmapSetWithMetadata(metadata, metadata);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestInconsistentArtist()
        {
            var metadata1 = createMetadata("Artist One", "Test Title", "Test Source", "Test Creator", "tag1 tag2");
            var metadata2 = createMetadata("Artist Two", "Test Title", "Test Source", "Test Creator", "tag1 tag2");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields);
            Assert.That(issues[0].ToString(), Contains.Substring("Inconsistent artist fields"));
            Assert.That(issues[0].ToString(), Contains.Substring("Artist One"));
            Assert.That(issues[0].ToString(), Contains.Substring("Artist Two"));
        }

        [Test]
        public void TestInconsistentTitle()
        {
            var metadata1 = createMetadata("Test Artist", "Title One", "Test Source", "Test Creator", "tag1 tag2");
            var metadata2 = createMetadata("Test Artist", "Title Two", "Test Source", "Test Creator", "tag1 tag2");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields);
            Assert.That(issues[0].ToString(), Contains.Substring("Inconsistent title fields"));
        }

        [Test]
        public void TestInconsistentUnicodeArtist()
        {
            var metadata1 = createMetadata("Test Artist", "Test Title", "Test Source", "Test Creator", "tag1 tag2", unicodeArtist: "Test Unicode Artist 1");
            var metadata2 = createMetadata("Test Artist", "Test Title", "Test Source", "Test Creator", "tag1 tag2", unicodeArtist: "Test Unicode Artist 2");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields);
            Assert.That(issues[0].ToString(), Contains.Substring("Inconsistent unicode artist fields"));
        }

        [Test]
        public void TestInconsistentSource()
        {
            var metadata1 = createMetadata("Test Artist", "Test Title", "Source One", "Test Creator", "tag1 tag2");
            var metadata2 = createMetadata("Test Artist", "Test Title", "Source Two", "Test Creator", "tag1 tag2");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields);
            Assert.That(issues[0].ToString(), Contains.Substring("Inconsistent source fields"));
        }

        [Test]
        public void TestInconsistentCreator()
        {
            var metadata1 = createMetadata("Test Artist", "Test Title", "Test Source", "Creator One", "tag1 tag2");
            var metadata2 = createMetadata("Test Artist", "Test Title", "Test Source", "Creator Two", "tag1 tag2");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields);
            Assert.That(issues[0].ToString(), Contains.Substring("Inconsistent creator fields"));
        }

        [Test]
        public void TestInconsistentTags()
        {
            var metadata1 = createMetadata("Test Artist", "Test Title", "Test Source", "Test Creator", "tag1 tag2 tag3");
            var metadata2 = createMetadata("Test Artist", "Test Title", "Test Source", "Test Creator", "tag1 tag4 tag5");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentMetadata.IssueTemplateInconsistentTags);
            Assert.That(issues[0].ToString(), Contains.Substring("Inconsistent tags"));
            Assert.That(issues[0].ToString(), Contains.Substring("tag2 tag3 tag4 tag5"));
        }

        [Test]
        public void TestMultipleInconsistencies()
        {
            var metadata1 = createMetadata("Artist One", "Title One", "Test Source", "Test Creator", "tag1 tag2");
            var metadata2 = createMetadata("Artist Two", "Title Two", "Test Source", "Test Creator", "tag3 tag4");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(3)); // artist, title, tags
            Assert.That(issues.Count(i => i.Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields), Is.EqualTo(2));
            Assert.That(issues.Count(i => i.Template is CheckInconsistentMetadata.IssueTemplateInconsistentTags), Is.EqualTo(1));
        }

        [Test]
        public void TestSingleDifficulty()
        {
            var metadata = createMetadata("Test Artist", "Test Title", "Test Source", "Test Creator", "tag1 tag2");
            var beatmaps = createBeatmapSetWithMetadata(metadata);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestEmptyStringFieldsAreConsistent()
        {
            var metadata1 = createMetadata("Test Artist", "Test Title", "", "Test Creator", "");
            var metadata2 = createMetadata("Test Artist", "Test Title", "", "Test Creator", "");
            var beatmaps = createBeatmapSetWithMetadata(metadata1, metadata2);
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        private BeatmapMetadata createMetadata(string artist, string title, string source, string creator, string tags, string unicodeArtist = "", string unicodeTitle = "")
        {
            return new BeatmapMetadata(new RealmUser { Username = creator })
            {
                Artist = artist,
                Title = title,
                Source = source,
                Tags = tags,
                ArtistUnicode = unicodeArtist,
                TitleUnicode = unicodeTitle
            };
        }

        private IBeatmap[] createBeatmapSetWithMetadata(params BeatmapMetadata[] metadata)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[metadata.Length];

            for (int i = 0; i < metadata.Length; i++)
            {
                beatmaps[i] = createBeatmapWithMetadata(metadata[i], $"Difficulty {i + 1}");
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
            }

            // Configure the beatmapset to contain all the beatmap infos
            foreach (var beatmap in beatmaps)
                beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);

            return beatmaps;
        }

        private Beatmap createBeatmapWithMetadata(BeatmapMetadata metadata, string difficultyName)
        {
            return new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Metadata = metadata
                }
            };
        }

        private BeatmapVerifierContext createContextWithMultipleDifficulties(IBeatmap currentBeatmap, IBeatmap[] allDifficulties)
        {
            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap), currentBeatmap);
            var verifiedOtherBeatmaps = allDifficulties.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, DifficultyRating.ExpertPlus);
        }
    }
}
