// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class TimingDistribution
    {
        public readonly int[] Bins;
        public readonly double BinSize;

        public TimingDistribution(int binCount, double binSize)
        {
            Bins = new int[binCount];
            BinSize = binSize;
        }
    }
}
