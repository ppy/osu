// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckMissingGenreLanguageTest
    {
        private CheckMissingGenreLanguage check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckMissingGenreLanguage();
        }

        [Test]
        public void TestHasGenreAndLanguage()
        {
            var beatmap = createBeatmapWithTags("rock english instrumental");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestHasGenreOnly()
        {
            var beatmap = createBeatmapWithTags("electronic pop");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage);
        }

        [Test]
        public void TestHasLanguageOnly()
        {
            var beatmap = createBeatmapWithTags("japanese instrumental");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckMissingGenreLanguage.IssueTemplateMissingGenre);
        }

        [Test]
        public void TestMissingBoth()
        {
            var beatmap = createBeatmapWithTags("tag1 tag2 tag3");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckMissingGenreLanguage.IssueTemplateMissingGenre));
            Assert.That(issues.Any(issue => issue.Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage));
        }

        [Test]
        public void TestMultiWordGenreHipHop()
        {
            var beatmap = createBeatmapWithTags("hip hop music");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage);
        }

        [Test]
        public void TestScatteredMultiWordGenre()
        {
            var beatmap = createBeatmapWithTags("video hip game hop ost");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage);
        }

        [Test]
        public void TestCaseInsensitive()
        {
            var beatmap = createBeatmapWithTags("ROCK JAPANESE");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestMixedCase()
        {
            var beatmap = createBeatmapWithTags("Rock Japanese");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestSingleWordGenre()
        {
            var beatmap = createBeatmapWithTags("electronic");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage);
        }

        [Test]
        public void TestSingleWordLanguage()
        {
            var beatmap = createBeatmapWithTags("instrumental");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckMissingGenreLanguage.IssueTemplateMissingGenre);
        }

        [Test]
        public void TestEmptyTags()
        {
            var beatmap = createBeatmapWithTags("");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckMissingGenreLanguage.IssueTemplateMissingGenre));
            Assert.That(issues.Any(issue => issue.Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage));
        }

        [Test]
        public void TestPartialMultiWordMatch()
        {
            // Should not match if only one word is found
            var beatmap = createBeatmapWithTags("hip music");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckMissingGenreLanguage.IssueTemplateMissingGenre));
            Assert.That(issues.Any(issue => issue.Template is CheckMissingGenreLanguage.IssueTemplateMissingLanguage));
        }

        [Test]
        public void TestGenreAndLanguageWithExtraTags()
        {
            var beatmap = createBeatmapWithTags("tag1 rock tag2 english tag3");
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            Assert.That(check.Run(context), Is.Empty);
        }

        private IBeatmap createBeatmapWithTags(string tags)
        {
            return new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { Tags = tags }
                }
            };
        }
    }
}
