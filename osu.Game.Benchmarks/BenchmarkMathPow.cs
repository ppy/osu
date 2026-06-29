// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using BenchmarkDotNet.Attributes;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Benchmarks
{
    public class BenchmarkMathPow : BenchmarkTest
    {
        [Params(0, 1, 1.25, 2.0, 3, 5)]
        public double Exponent { get; set; }

        [Benchmark]
        public double MathPow()
        {
            return Math.Pow(1.299995, Exponent);
        }

        [Benchmark]
        public double DiffUtilsPowDouble()
        {
            return DiffUtils.Pow(1.299995, Exponent);
        }

        [Benchmark]
        public double DiffUtilsPowInt()
        {
            if ((int)Exponent != Exponent)
                throw new NotSupportedException();

            return DiffUtils.Pow(1.299995, (int)Exponent);
        }
    }
}
