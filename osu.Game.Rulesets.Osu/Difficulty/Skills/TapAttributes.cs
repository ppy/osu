
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
