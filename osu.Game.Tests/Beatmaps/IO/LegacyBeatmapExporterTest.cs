// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;
using MemoryStream = System.IO.MemoryStream;

namespace osu.Game.Tests.Beatmaps.IO
{
    [HeadlessTest]
    public partial class LegacyBeatmapExporterTest : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Test]
        public void TestObjectsSnappedAfterTruncatingExport()
        {
            IWorkingBeatmap beatmap = null!;
            MemoryStream outStream = null!;

            // Ensure importer encoding is correct
            AddStep("import beatmap", () => beatmap = importBeatmapFromArchives(@"decimal-timing-beatmap.olz"));
            AddAssert("timing point has decimal offset", () => beatmap.Beatmap.ControlPointInfo.TimingPoints[0].Time, () => Is.EqualTo(284.725).Within(0.001));
            AddAssert("kiai has decimal offset", () => beatmap.Beatmap.ControlPointInfo.EffectPoints[0].Time, () => Is.EqualTo(28520.019).Within(0.001));
            AddAssert("hit object has decimal offset", () => beatmap.Beatmap.HitObjects[0].StartTime, () => Is.EqualTo(28520.019).Within(0.001));

            // Ensure exporter legacy conversion is correct
            AddStep("export", () =>
            {
                outStream = new MemoryStream();

                new LegacyBeatmapExporter(LocalStorage)
                    .ExportToStream((BeatmapSetInfo)beatmap.BeatmapInfo.BeatmapSet!, outStream, null);
            });

            AddStep("import beatmap again", () => beatmap = importBeatmapFromStream(outStream));
            AddAssert("timing point has truncated offset", () => beatmap.Beatmap.ControlPointInfo.TimingPoints[0].Time, () => Is.EqualTo(284).Within(0.001));
            AddAssert("kiai is snapped", () => beatmap.Beatmap.ControlPointInfo.EffectPoints[0].Time, () => Is.EqualTo(28519).Within(0.001));
            AddAssert("hit object is snapped", () => beatmap.Beatmap.HitObjects[0].StartTime, () => Is.EqualTo(28519).Within(0.001));
        }

        [Test]
        public void TestFractionalObjectCoordinatesRounded()
        {
            IWorkingBeatmap beatmap = null!;
            MemoryStream outStream = null!;

            // Ensure importer encoding is correct
            AddStep("import beatmap", () => beatmap = importBeatmapFromArchives(@"fractional-coordinates.olz"));
            AddAssert("second slider has fractional position",
                () => ((IHasXPosition)beatmap.Beatmap.HitObjects[1]).X,
                () => Is.EqualTo(-3.0517578E-05).Within(0.00001));
            AddAssert("second slider path has fractional coordinates",
                () => ((IHasPath)beatmap.Beatmap.HitObjects[1]).Path.ControlPoints[1].Position.X,
                () => Is.EqualTo(191.999939).Within(0.00001));
            AddAssert("second hit circle has fractional position",
                () => ((IHasYPosition)beatmap.Beatmap.HitObjects[3]).Y,
                () => Is.EqualTo(383.99997).Within(0.00001));

            // Ensure exporter legacy conversion is correct
            AddStep("export", () =>
            {
                outStream = new MemoryStream();

                new LegacyBeatmapExporter(LocalStorage)
                    .ExportToStream((BeatmapSetInfo)beatmap.BeatmapInfo.BeatmapSet!, outStream, null);
            });

            AddStep("import beatmap again", () => beatmap = importBeatmapFromStream(outStream));
            AddAssert("second slider is snapped",
                () => ((IHasXPosition)beatmap.Beatmap.HitObjects[1]).X,
                () => Is.EqualTo(0).Within(0.00001));
            AddAssert("second slider path is snapped",
                () => ((IHasPath)beatmap.Beatmap.HitObjects[1]).Path.ControlPoints[1].Position.X,
                () => Is.EqualTo(192).Within(0.00001));
            AddAssert("second hit circle is snapped",
                () => ((IHasYPosition)beatmap.Beatmap.HitObjects[3]).Y,
                () => Is.EqualTo(384).Within(0.00001));
        }

        [Test]
        public void TestExportStability()
        {
            IWorkingBeatmap beatmap = null!;
            MemoryStream firstExport = null!;
            MemoryStream secondExport = null!;

            // Ensure importer encoding is correct
            AddStep("import beatmap", () => beatmap = importBeatmapFromArchives(@"legacy-export-stability-test.olz"));
            AddStep("export once", () =>
            {
                firstExport = new MemoryStream();

                new LegacyBeatmapExporter(LocalStorage)
                    .ExportToStream((BeatmapSetInfo)beatmap.BeatmapInfo.BeatmapSet!, firstExport, null);
            });

            AddStep("import beatmap again", () => beatmap = importBeatmapFromStream(firstExport));
            AddStep("export again", () =>
            {
                secondExport = new MemoryStream();

                new LegacyBeatmapExporter(LocalStorage)
                    .ExportToStream((BeatmapSetInfo)beatmap.BeatmapInfo.BeatmapSet!, secondExport, null);
            });

            const string osu_filename = @"legacy export - stability test (spaceman_atlas) [].osu";

            AddAssert("exports are identical",
                () => getStringContentsOf(osu_filename, firstExport.GetBuffer()),
                () => Is.EqualTo(getStringContentsOf(osu_filename, secondExport.GetBuffer())));

            string getStringContentsOf(string filename, byte[] archiveBytes)
            {
                using var memoryStream = new MemoryStream(archiveBytes);
                using var archiveReader = new ZipArchiveReader(memoryStream);
                byte[] fileContent = archiveReader.GetStream(filename).ReadAllBytesToArray();
                return Encoding.UTF8.GetString(fileContent);
            }
        }

        private IWorkingBeatmap importBeatmapFromStream(Stream stream)
        {
            var imported = beatmaps.Import(new ImportTask(stream, "filename.osz")).GetResultSafely();
            return imported.AsNonNull().PerformRead(s => beatmaps.GetWorkingBeatmap(s.Beatmaps[0]));
        }

        private IWorkingBeatmap importBeatmapFromArchives(string filename)
        {
            var imported = beatmaps.Import(new ImportTask(TestResources.OpenResource($@"Archives/{filename}"), filename)).GetResultSafely();
            return imported.AsNonNull().PerformRead(s => beatmaps.GetWorkingBeatmap(s.Beatmaps[0]));
        }
    }
}
