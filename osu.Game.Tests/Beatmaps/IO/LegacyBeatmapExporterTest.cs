// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
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
