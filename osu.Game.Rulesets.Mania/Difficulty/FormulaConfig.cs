// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class FormulaConfig
    {
        public double RescaleHighThreshold = 10.421040776725855; // 9.0
        public double RescaleHighFactor = 1.713757153751726; // 1.0 / 1.2
        public double HitLeniencyBase = 0.3310181856698675;
        public double HitLeniencyOdMultiplier = 3.109121989522241;
        public double HitLeniencyOdBase = 56.59723759581402;
        public double SmoothingWindowMs = 500;
        public double AccuracySmoothingWindowMs = 400;
        public double ColumnActivityWindowMs = 150;
        public double KeyUsageWindowMs = 400;
        public double JackNerfCoefficient = 0.47020623513898313;
        public double JackNerfBase = 18.45757328394883;
        public double JackNerfPower = -33.918373694021355;
        public double StreamBoostMinRatio = 172.41458658810265;
        public double StreamBoostMaxRatio = 370.5574993562436;
        public double StreamBoostCoefficient = 4.8916799621709146E-8;
    }
}
