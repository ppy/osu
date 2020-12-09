// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double AimStrain;
        public double SpeedStrain;
        public double ApproachRate;
        public double OverallDifficulty;
        public int HitCircleCount;
        public int SpinnerCount;
    }
}
