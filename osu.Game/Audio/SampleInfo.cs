// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Audio
{
    /// <summary>
    /// Describes a gameplay sample.
    /// </summary>
    public class SampleInfo : ISampleInfo
    {
        private readonly string sampleName;

        public SampleInfo(string sampleName)
        {
            this.sampleName = sampleName;
        }

        public IEnumerable<string> LookupNames => new[] { sampleName };

        public int Volume { get; set; } = 100;
    }
}
