// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using BenchmarkDotNet.Attributes;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Benchmarks
{
    public class BenchmarkMod : BenchmarkTest
    {
        private OsuModDoubleTime mod = null!;

        [Params(1, 10, 100)]
        public int Times { get; set; }

        public override void SetUp()
        {
            base.SetUp();
            mod = new OsuModDoubleTime();
        }

        [Benchmark]
        public int ModHashCode()
        {
            var hashCode = new HashCode();

            for (int i = 0; i < Times; i++)
                hashCode.Add(mod);

            return hashCode.ToHashCode();
        }
    }
}
