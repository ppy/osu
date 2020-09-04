// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class TapAttributes
    {
        public double TapDifficulty;
        public double StreamNoteCount;
        public double MashedTapDifficulty;
        public List<Vector<double>> StrainHistory;
    }
}
