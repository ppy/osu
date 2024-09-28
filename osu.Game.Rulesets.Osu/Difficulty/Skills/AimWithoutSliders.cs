// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the Aim skill that includes sliders in it's calculation
    /// </summary>
    public class AimWithoutSliders : Aim
    {
        public AimWithoutSliders(Mod[] mods)
            : base(mods, false)
        {
        }
    }
}
