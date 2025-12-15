// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using BenchmarkDotNet.Attributes;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkDifficultyCalculation : BenchmarkTest
    {
        private DifficultyCalculator osuCalculator = null!;
        private DifficultyCalculator taikoCalculator = null!;
        private DifficultyCalculator catchCalculator = null!;
        private DifficultyCalculator maniaCalculator = null!;

        public override void SetUp()
        {
            using var resources = new DllResourceStore(typeof(TestResources).Assembly);

            using var archive = resources.GetStream("Resources/Archives/241526 Soleily - Renatus.osz");
            using var archiveReader = new ZipArchiveReader(archive);

            var osuBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (Gamu) [Insane].osu");
            var taikoBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (MMzz) [Oni].osu");
            var catchBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (Deif) [Salad].osu");
            var maniaBeatmap = readBeatmap(archiveReader, "Soleily - Renatus (ExPew) [Another].osu");

            osuCalculator = new OsuRuleset().CreateDifficultyCalculator(osuBeatmap);
            taikoCalculator = new TaikoRuleset().CreateDifficultyCalculator(taikoBeatmap);
            catchCalculator = new CatchRuleset().CreateDifficultyCalculator(catchBeatmap);
            maniaCalculator = new ManiaRuleset().CreateDifficultyCalculator(maniaBeatmap);
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
        public void CalculateDifficultyOsu() => osuCalculator.Calculate();

        [Benchmark]
        public void CalculateDifficultyTaiko() => taikoCalculator.Calculate();

        [Benchmark]
        public void CalculateDifficultyCatch() => catchCalculator.Calculate();

        [Benchmark]
        public void CalculateDifficultyMania() => maniaCalculator.Calculate();

        [Benchmark]
        public void CalculateDifficultyOsuHundredTimes()
        {
            for (int i = 0; i < 100; i++)
            {
                osuCalculator.Calculate();
            }
        }
    }
}
