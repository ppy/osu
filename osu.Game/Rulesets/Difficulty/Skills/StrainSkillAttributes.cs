// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public class StrainSkillAttributes : ISkillAttributes
    {
        public required double Difficulty { get; init; }
        public required List<double> ObjectDifficulties { get; init; }
        public required List<StrainPeak> StrainPeaks { get; init; }
        public required double TopWeightedStrainsCount { get; init; }
    }
}
