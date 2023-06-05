// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public partial class TestSceneBeatmapSkinResources : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Test]
        public void TestRetrieveOggAudio()
        {
            IWorkingBeatmap beatmap = null!;

            AddStep("import beatmap", () => beatmap = importBeatmapFromArchives(@"ogg-beatmap.osz"));
            AddAssert("sample is non-null", () => beatmap.Skin.GetSample(new SampleInfo(@"sample")) != null);
            AddAssert("track is non-null", () =>
            {
                using (var track = beatmap.LoadTrack())
                    return track is not TrackVirtual;
            });
        }

        [Test]
        public void TestRetrievalWithConflictingFilenames()
        {
            IWorkingBeatmap beatmap = null!;

            AddStep("import beatmap", () => beatmap = importBeatmapFromArchives(@"conflicting-filenames-beatmap.osz"));
            AddAssert("texture is non-null", () => beatmap.Skin.GetTexture(@"spinner-osu") != null);
            AddAssert("sample is non-null", () => beatmap.Skin.GetSample(new SampleInfo(@"spinner-osu")) != null);
        }

        private IWorkingBeatmap importBeatmapFromArchives(string filename)
        {
            var imported = beatmaps.Import(new ImportTask(TestResources.OpenResource($@"Archives/{filename}"), filename)).GetResultSafely();
            return imported.AsNonNull().PerformRead(s => beatmaps.GetWorkingBeatmap(s.Beatmaps[0]));
        }
    }
}
