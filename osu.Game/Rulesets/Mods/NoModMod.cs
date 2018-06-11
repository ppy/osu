// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Indicates a type of mod that doesn't do anything.
    /// </summary>
    public sealed class NoModMod : Mod
    {
        public override string Name => "No Mod";
        public override string ShortenedName => "NM";
        public override double ScoreMultiplier => 1;
    }
}
