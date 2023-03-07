// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Game.Utils;

namespace osu.Game.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkCompactList
    {
        [Benchmark]
        public int AddRandom()
        {
            CompactList list = new CompactList();

            for (int i = 0; i < 1_000_000; i++)
                list.Add(i);

            return list.Count;
        }

        [Benchmark]
        public int AddDuplicate()
        {
            CompactList list = new CompactList();

            for (int i = 0; i < 1_000_000; i++)
                list.Add(1);

            return list.Count;
        }
    }
}
