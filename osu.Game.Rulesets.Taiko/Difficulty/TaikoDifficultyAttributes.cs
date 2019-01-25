// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyAttributes : DifficultyAttributes
    {
        public double GreatHitWindow;
        public int MaxCombo;

        public TaikoDifficultyAttributes(Mod[] mods, double starRating)
            : base(mods, starRating)
        {
        }
    }
}
