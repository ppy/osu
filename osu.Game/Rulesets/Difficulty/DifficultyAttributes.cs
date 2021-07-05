// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    public class DifficultyAttributes
    {
        public Mod[] Mods { get; set; }
        public Skill[] Skills { get; set; }

        public double StarRating { get; set; }
        public int MaxCombo { get; set; }

        public DifficultyAttributes()
        {
        }

        public DifficultyAttributes(Mod[] mods, Skill[] skills, double starRating)
        {
            Mods = mods;
            Skills = skills;
            StarRating = starRating;
        }
    }
}
