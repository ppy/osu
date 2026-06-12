// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Benchmarks
{
    public class BenchmarkScoreMultiplierCalculator : BenchmarkTest
    {
        private ScoreMultiplierCalculator calculator = null!;

        [Params(1, 10, 100)]
        public int Times { get; set; }

        public record ModTestCase(string Description, IEnumerable<Mod> Mods)
        {
            public override string ToString() => Description;
        }

        public static IEnumerable<ModTestCase> ValuesForMods =>
        [
            new ModTestCase("no mods", []),
            new ModTestCase("single mod", [new OsuModHardRock()]),
            new ModTestCase("single mod 2", [new OsuModEasy()]),
            new ModTestCase("multiple mods", [new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime()]),
            new ModTestCase("mods with adjusted settings", [
                new OsuModDoubleTime { SpeedChange = { Value = 2 } },
                new OsuModHidden { OnlyFadeApproachCircles = { Value = true } },
                new OsuModHardRock()
            ]),
        ];

        [ParamsSource(nameof(ValuesForMods))]
        public ModTestCase Mods { get; set; } = null!;

        public override void SetUp()
        {
            base.SetUp();
            calculator = new OsuRuleset().CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));
        }

        [Benchmark]
        public double ViaCalculator()
            => viaCalculator(Times, Mods);

        [Test]
        public void ViaCalculator([Values(100)] int times, [ValueSource(nameof(ValuesForMods))] ModTestCase mods)
            => viaCalculator(times, mods);

        private double viaCalculator(int times, ModTestCase mods)
        {
            double scoreMultiplier = 1;

            for (int i = 0; i < times; ++i)
                scoreMultiplier = calculator.CalculateFor(mods.Mods);

            return scoreMultiplier;
        }
    }
}
