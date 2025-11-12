// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class FormulaConfig
    {
        public double RescaleHighThreshold = 9.648018295800334;
        public double RescaleHighFactor = 1.4441598782471803;
        public double HitLeniencyBase = 0.3310181856698675;
        public double HitLeniencyOdMultiplier = 3.360810802810689;
        public double HitLeniencyOdBase = 56.59723759581402;
        public double SmoothingWindowMs = 490.74808264043077;
        public double AccuracySmoothingWindowMs = 383.2749819546263;
        public double ColumnActivityWindowMs = 231.11564703753132;
        public double KeyUsageWindowMs = 310.92348654941304;
        public double JackNerfCoefficient = 0.47020623513898313;
        public double JackNerfBase = 18.441418584326687;
        public double JackNerfPower = -33.72322163074505;
        public double StreamBoostMinRatio = 172.41458658810265;
        public double StreamBoostMaxRatio = 384.7187521409616;
        public double StreamBoostCoefficient = 6.58242983168459E-8;
    }
}
