// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using osu.Game.Utils;

namespace osu.Game.Benchmarks
{
    public class BenchmarkStringComparison
    {
        private string[] strings = null!;

        [GlobalSetup]
        public void GlobalSetUp()
        {
            strings = new string[10000];

            for (int i = 0; i < strings.Length; ++i)
                strings[i] = Guid.NewGuid().ToString();

            for (int i = 0; i < strings.Length; ++i)
            {
                if (i % 2 == 0)
                    strings[i] = strings[i].ToUpperInvariant();
            }
        }

        [Benchmark]
        public void OrdinalIgnoreCase() => compare(StringComparer.OrdinalIgnoreCase);

        [Benchmark]
        public void OrdinalSortByCase() => compare(OrdinalSortByCaseStringComparer.INSTANCE);

        [Benchmark]
        public void InvariantCulture() => compare(StringComparer.InvariantCulture);

        private void compare(IComparer<string> comparer)
        {
            for (int i = 0; i < strings.Length; ++i)
            {
                for (int j = i + 1; j < strings.Length; ++j)
                    _ = comparer.Compare(strings[i], strings[j]);
            }
        }
    }
}
