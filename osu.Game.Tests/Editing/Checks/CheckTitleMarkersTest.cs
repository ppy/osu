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
    public class CheckTitleMarkersTest
    {
        private CheckTitleMarkers check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckTitleMarkers();

            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "Egao no Kanata",
                        TitleUnicode = "エガオノカナタ"
                    }
                }
            };
        }

        [Test]
        public void TestNoTitleMarkers()
        {
            var issues = check.Run(getContext(beatmap)).ToList();
            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestTvSizeMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (TV Size)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (TV Size)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedTvSizeMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (tv size)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (tv size)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestGameVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Game Ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Game Ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedGameVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (game ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (game ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestShortVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Short Ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Short Ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedShortVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (short ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (short ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestCutVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Cut Ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Cut Ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedCutVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (cut ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (cut ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestSpedUpVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Sped Up Ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Sped Up Ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedSpedUpVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (sped up ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (sped up ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestNightcoreMixMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Nightcore Mix)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Nightcore Mix)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedNightcoreMixMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (nightcore mix)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (nightcore mix)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestSpedUpCutVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Sped Up & Cut Ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Sped Up & Cut Ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedSpedUpCutVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (sped up & cut ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (sped up & cut ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        [Test]
        public void TestNightcoreCutVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (Nightcore & Cut Ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (Nightcore & Cut Ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestMalformedNightcoreCutVerMarker()
        {
            beatmap.BeatmapInfo.Metadata.Title += " (nightcore & cut ver.)";
            beatmap.BeatmapInfo.Metadata.TitleUnicode += " (nightcore & cut ver.)";

            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckTitleMarkers.IssueTemplateIncorrectMarker));
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap)
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}