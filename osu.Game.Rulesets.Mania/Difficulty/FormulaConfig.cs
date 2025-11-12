// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class FormulaConfig
    {
        public double RescaleHighThreshold = 10.351016531598967;
        public double RescaleHighFactor = 1.4441598782471803;
        public double HitLeniencyBase = 0.3310181856698675;
        public double HitLeniencyOdMultiplier = 3.109121989522241;
        public double HitLeniencyOdBase = 56.59723759581402;
        public double SmoothingWindowMs = 490.8525597893018;
        public double AccuracySmoothingWindowMs = 338.2749819546263;
        public double ColumnActivityWindowMs = 231.11564703753132;
        public double KeyUsageWindowMs = 340.92348654941304;
        public double JackNerfCoefficient = 0.47020623513898313;
        public double JackNerfBase = 18.45757328394883;
        public double JackNerfPower = -33.918373694021355;
        public double StreamBoostMinRatio = 172.41458658810265;
        public double StreamBoostMaxRatio = 370.5574993562436;
        public double StreamBoostCoefficient = 4.8916799621709146E-8;
    }
}
