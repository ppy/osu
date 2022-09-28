// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModVisualAdjusts : ModWithVisibilityAdjustment
    {
        public override string Name => "Visual Adjusts";
        public override LocalisableString Description => "Adjust some gameplay elements that can bring some visual challenge.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "VA";
        public override ModType Type => ModType.Conversion;
    }
}
