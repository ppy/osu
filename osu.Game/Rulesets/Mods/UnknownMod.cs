// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public sealed class UnknownMod : Mod
    {
        /// <summary>
        /// The acronym of the mod which could not be resolved.
        /// </summary>
        public readonly string OriginalAcronym;

        public override string Name => $"Unknown mod ({OriginalAcronym})";
        public override string Acronym => $"{OriginalAcronym}??";
        public override LocalisableString Description => "This mod could not be resolved by the game.";
        public override double ScoreMultiplier => 0;

        public override bool UserPlayable => false;
        public override bool ValidForMultiplayer => false;
        public override bool ValidForMultiplayerAsFreeMod => false;

        public override ModType Type => ModType.System;

        public UnknownMod(string acronym)
        {
            OriginalAcronym = acronym;
        }

        public override Mod DeepClone() => new UnknownMod(OriginalAcronym);
    }
}
