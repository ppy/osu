// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace osu.Game.Benchmarks
{
    [TestFixture]
    [MemoryDiagnoser]
    public abstract class BenchmarkTest
    {
        [GlobalSetup]
        [OneTimeSetUp]
        public virtual void SetUp()
        {
        }

        [Test]
        public void RunBenchmark() => BenchmarkRunner.Run(GetType());
    }
}
