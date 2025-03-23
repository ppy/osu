// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using BenchmarkDotNet.Attributes;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkDifficultyCalculation : BenchmarkTest
    {
        private WorkingBeatmap osuBeatmap = null!;
        private WorkingBeatmap taikoBeatmap = null!;
        private WorkingBeatmap catchBeatmap = null!;
        private WorkingBeatmap maniaBeatmap = null!;

        public override void SetUp()
        {
            using var resources = new DllResourceStore(typeof(TestResources).Assembly);

            using var archive = resources.GetStream("Resources/Archives/241526 Soleily - Renatus.osz");
            using var archiveReader = new ZipArchiveReader(archive);

            osuBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (Gamu) [Insane].osu");
            taikoBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (MMzz) [Oni].osu");
            catchBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (Deif) [Salad].osu");
            maniaBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (ExPew) [Another].osu");
        }

        private WorkingBeatmap readBeatmap(ZipArchiveReader archiveReader, string beatmapName)
        {
            using var beatmapStream = new MemoryStream();
            archiveReader.GetStream(beatmapName).CopyTo(beatmapStream);

            beatmapStream.Seek(0, SeekOrigin.Begin);
            using var reader = new LineBufferedReader(beatmapStream);

            var decoder = Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader);
            return new FlatWorkingBeatmap(decoder.Decode(reader));
        }

        [Benchmark]
        public void CalculateDifficultyOsu() => new OsuRuleset().CreateDifficultyCalculator(osuBeatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyTaiko() => new TaikoRuleset().CreateDifficultyCalculator(taikoBeatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyCatch() => new CatchRuleset().CreateDifficultyCalculator(catchBeatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyMania() => new ManiaRuleset().CreateDifficultyCalculator(maniaBeatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyOsuHundredTimes()
        {
            var diffcalc = new OsuRuleset().CreateDifficultyCalculator(osuBeatmap);

            for (int i = 0; i < 100; i++)
            {
                diffcalc.Calculate();
            }
        }
    }
}
