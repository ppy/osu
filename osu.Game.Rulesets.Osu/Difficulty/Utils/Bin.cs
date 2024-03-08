// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public struct Bin
    {
        public double Difficulty;
        public double Count;

        public double HitProbability(double skill) => SpecialFunctions.Erf(skill / (Math.Sqrt(2) * Difficulty));

        public double FcProbability(double skill) => Math.Pow(HitProbability(skill), Count);
    }
}
