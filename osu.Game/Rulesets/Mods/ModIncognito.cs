// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModIncognito : Mod
    {
        public override string Name => "Incognito";
        public override LocalisableString Description => "Obscure some gameplay elements.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "IC";
        public override ModType Type => ModType.Conversion;
    }
}
