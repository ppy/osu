// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Mod that colours hitobjects based on the musical division they are on
    /// </summary>
    public class ModSnapColour : Mod
    {
        public override string Name => "Snap Colour";
        public override string Acronym => "SC";
        public override LocalisableString Description => "Colours hit objects based on the rhythm.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;
    }
}
