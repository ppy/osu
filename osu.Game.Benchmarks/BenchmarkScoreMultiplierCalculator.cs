// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
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
            calculator = new OsuRuleset().CreateScoreMultiplierCalculator();
        }

        [Benchmark]
        public double ViaModScoreMultiplier()
        {
            double scoreMultiplier = 1;

            for (int i = 0; i < Times; ++i)
            {
                scoreMultiplier = 1;

                foreach (var mod in Mods.Mods)
                    scoreMultiplier *= mod.ScoreMultiplier;
            }

            return scoreMultiplier;
        }

        [Benchmark]
        public double ViaCalculator()
        {
            double scoreMultiplier = 1;

            for (int i = 0; i < Times; ++i)
                scoreMultiplier = calculator.CalculateFor(Mods.Mods);

            return scoreMultiplier;
        }
    }
}
