// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class ArgonWedgePieceStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.ArgonWedgePiece";

        /// <summary>
        /// "Inverted shear"
        /// </summary>
        public static LocalisableString InvertedShear => new TranslatableString(getKey(@"inverted_shear"), @"Inverted shear");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
