// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Audio
{
    /// <summary>
    /// Describes a gameplay sample.
    /// </summary>
    public class SampleInfo : ISampleInfo, IEquatable<SampleInfo>
    {
        private readonly string[] sampleNames;

        public SampleInfo(params string[] sampleNames)
        {
            this.sampleNames = sampleNames;
            Array.Sort(sampleNames);
        }

        public IEnumerable<string> LookupNames => sampleNames;

        public int Volume { get; } = 100;

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StructuralComparisons.StructuralEqualityComparer.GetHashCode(sampleNames),
                Volume);
        }

        public bool Equals(SampleInfo other)
            => other != null && sampleNames.SequenceEqual(other.sampleNames);

        public override bool Equals(object obj)
            => obj is SampleInfo other && Equals(other);
    }
}
