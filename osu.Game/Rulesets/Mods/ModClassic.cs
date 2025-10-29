// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModClassic : Mod
    {
        public override string Name => "Classic";

        public override string Acronym => "CL";

        public override double ScoreMultiplier => 0.96;

        public override IconUsage? Icon => OsuIcon.ModClassic;

        public override LocalisableString Description => "Feeling nostalgic?";

        public override ModType Type => ModType.Conversion;

        public sealed override bool Ranked => false;

        public sealed override bool ValidForFreestyleAsRequiredMod => false;
    }
}
