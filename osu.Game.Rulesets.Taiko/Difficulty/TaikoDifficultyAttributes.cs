// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyAttributes : DifficultyAttributes
    {
        public double StaminaStrain { get; set; }
        public double RhythmStrain { get; set; }
        public double ColourStrain { get; set; }
        public double ApproachRate { get; set; }
        public double GreatHitWindow { get; set; }
    }
}
