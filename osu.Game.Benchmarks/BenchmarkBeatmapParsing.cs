// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using BenchmarkDotNet.Attributes;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkBeatmapParsing : BenchmarkTest
    {
        private readonly MemoryStream beatmapStream = new MemoryStream();

        public override void SetUp()
        {
            using (var resources = new DllResourceStore(typeof(TestResources).Assembly))
            using (var archive = resources.GetStream("Resources/Archives/241526 Soleily - Renatus.osz"))
            using (var reader = new ZipArchiveReader(archive))
                reader.GetStream("Soleily - Renatus (Gamu) [Insane].osu").CopyTo(beatmapStream);
        }

        [Benchmark]
        public Beatmap BenchmarkBundledBeatmap()
        {
            beatmapStream.Seek(0, SeekOrigin.Begin);
            var reader = new LineBufferedReader(beatmapStream); // no disposal

            var decoder = Decoder.GetDecoder<Beatmap>(reader);
            return decoder.Decode(reader);
        }
    }
}
