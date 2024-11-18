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
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkDifficultyCalculation : BenchmarkTest
    {
        private FlatWorkingBeatmap beatmap = null!;

        private IDifficultyAttributes osuAttributes = null!;
        private IDifficultyAttributes taikoAttributes = null!;
        private IDifficultyAttributes catchAttributes = null!;
        private IDifficultyAttributes maniaAttributes = null!;

        public override void SetUp()
        {
            using var resources = new DllResourceStore(typeof(TestResources).Assembly);
            using var archive = resources.GetStream("Resources/Archives/241526 Soleily - Renatus.osz");
            using var zipReader = new ZipArchiveReader(archive);

            using var beatmapStream = new MemoryStream();
            zipReader.GetStream("Soleily - Renatus (Gamu) [Insane].osu").CopyTo(beatmapStream);
            beatmapStream.Seek(0, SeekOrigin.Begin);
            var reader = new LineBufferedReader(beatmapStream);
            var decoder = Decoder.GetDecoder<Beatmap>(reader);

            beatmap = new FlatWorkingBeatmap(decoder.Decode(reader));

            // Prepare difficulty attributes for an isolated performance calculation in every mode.
            osuAttributes = DifficultyOsu();
            taikoAttributes = DifficultyTaiko();
            catchAttributes = DifficultyCatch();
            maniaAttributes = DifficultyMania();
        }

        [Benchmark]
        public IDifficultyAttributes DifficultyOsu() => new OsuRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public IDifficultyAttributes DifficultyTaiko() => new TaikoRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public IDifficultyAttributes DifficultyCatch() => new CatchRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public IDifficultyAttributes DifficultyMania() => new ManiaRuleset().CreateDifficultyCalculator(beatmap).Calculate();

        [Benchmark]
        public void PerformanceOsu()
        {
            Ruleset ruleset = new OsuRuleset();
            ScoreInfo score = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo);
            ruleset.CreatePerformanceCalculator()!.Calculate(score, osuAttributes);
        }

        [Benchmark]
        public void PerformanceTaiko()
        {
            Ruleset ruleset = new TaikoRuleset();
            ScoreInfo score = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo);
            ruleset.CreatePerformanceCalculator()!.Calculate(score, taikoAttributes);
        }

        [Benchmark]
        public void PerformanceCatch()
        {
            Ruleset ruleset = new CatchRuleset();
            ScoreInfo score = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo);
            ruleset.CreatePerformanceCalculator()!.Calculate(score, catchAttributes);
        }

        [Benchmark]
        public void PerformanceMania()
        {
            Ruleset ruleset = new ManiaRuleset();
            ScoreInfo score = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo);
            ruleset.CreatePerformanceCalculator()!.Calculate(score, maniaAttributes);
        }
    }
}
