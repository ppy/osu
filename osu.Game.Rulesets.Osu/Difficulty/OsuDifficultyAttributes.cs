// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double MissStarRatingIncrement;

        public double AimStrain;
        public IList<double> AimComboStarRatings;
        public IList<double> AimMissCounts;

        public double SpeedStrain;
        public IList<double> SpeedComboStarRatings;
        public IList<double> SpeedMissCounts;

        public double ApproachRate;
        public double OverallDifficulty;
        public int MaxCombo;

        public IList<OsuHitObjectDifficulty> HitObjectDifficulties; // only used for charts
    }
}
