// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using BenchmarkDotNet.Attributes;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkDifficultyCalculation : BenchmarkTest
    {
        private readonly MemoryStream beatmapStream = new MemoryStream();

        private Beatmap beatmap = null!;

        private static readonly Ruleset[] rulesets =
        {
            new OsuRuleset(),
            new TaikoRuleset(),
            new CatchRuleset(),
            new ManiaRuleset()
        };

        private IBeatmap[] convertedBeatmaps = null!;

        public override void SetUp()
        {
            using (var resources = new DllResourceStore(typeof(TestResources).Assembly))
            using (var archive = resources.GetStream("Resources/Archives/241526 Soleily - Renatus.osz"))
            using (var reader = new ZipArchiveReader(archive))
                reader.GetStream("Soleily - Renatus (Gamu) [Insane].osu").CopyTo(beatmapStream);

            beatmapStream.Seek(0, SeekOrigin.Begin);
            var lineBufferedReader = new LineBufferedReader(beatmapStream); // no disposal

            var decoder = Decoder.GetDecoder<Beatmap>(lineBufferedReader);
            beatmap = decoder.Decode(lineBufferedReader);

            convertedBeatmaps = new IBeatmap[4];

            for (int i = 0; i < rulesets.Length; i++)
            {
                var ruleset = rulesets[i];
                convertedBeatmaps[i] = ruleset.CreateBeatmapConverter(beatmap).Convert();
            }
        }

        [Benchmark]
        public void BenchmarkOsuDifficultyCalculation() => runFor(0);

        [Benchmark]
        public void BenchmarkTaikoDifficultyCalculation() => runFor(1);

        [Benchmark]
        public void BenchmarkCatchDifficultyCalculation() => runFor(2);

        [Benchmark]
        public void BenchmarkManiaDifficultyCalculation() => runFor(3);

        private void runFor(int legacyId) => rulesets[legacyId].CreateDifficultyCalculator(new FlatFileWorkingBeatmap(convertedBeatmaps[legacyId])).Calculate();
    }
}
