// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps
{
    public class BeatmapShortInfo : IEquatable<BeatmapShortInfo>
    {
        public StarDifficulty StarDifficulty;
        public float CircleSize;
        public float DrainRate;
        public float ApproachRate;
        public float OverallDifficulty;
        public double BPM;
        public bool Equals(BeatmapShortInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return StarDifficulty.Stars == other.StarDifficulty.Stars &&
                   CircleSize.Equals(other.CircleSize) &&
                   DrainRate.Equals(other.DrainRate) &&
                   ApproachRate.Equals(other.ApproachRate) &&
                   OverallDifficulty.Equals(other.OverallDifficulty) &&
                   BPM.Equals(other.BPM);
        }
    }
}
