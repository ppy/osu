// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using BenchmarkDotNet.Attributes;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkDifficultyCalculation : BenchmarkTest
    {
        private WorkingBeatmap beatmap = null!;

        public override void SetUp()
        {
            using var resources = new DllResourceStore(typeof(TestResources).Assembly);

            using var beatmapStream = resources.GetStream("Resources/Within Temptation - The Unforgiving (Armin) [Marathon].osu");

            beatmapStream.Seek(0, SeekOrigin.Begin);
            using var reader = new LineBufferedReader(beatmapStream);

            var decoder = Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader);
            beatmap = new FlatWorkingBeatmap(decoder.Decode(reader));
        }

        [Benchmark]
        public void CalculateDifficultyOsu() => new OsuRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyTaiko() => new TaikoRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyCatch() => new CatchRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public void CalculateDifficultyMania() => new ManiaRuleset().CreateDifficultyCalculator(beatmap).Calculate();
    }
}
