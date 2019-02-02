// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    public class DifficultyAttributes
    {
        public readonly Mod[] Mods;
        public readonly double StarRating;

        public DifficultyAttributes(Mod[] mods, double starRating)
        {
            Mods = mods;
            StarRating = starRating;
        }
    }
}
