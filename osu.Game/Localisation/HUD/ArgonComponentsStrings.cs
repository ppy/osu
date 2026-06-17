// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class ArgonComponentsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.ArgonAccuracyCounter";

        /// <summary>
        /// "Wireframe opacity"
        /// </summary>
        public static LocalisableString WireframeOpacity => new TranslatableString(getKey(@"wireframe_opacity"), @"Wireframe opacity");

        /// <summary>
        /// "Controls the opacity of the wireframes behind the digits."
        /// </summary>
        public static LocalisableString WireframeOpacityDescription => new TranslatableString(getKey(@"wireframe_opacity_decription"), @"Controls the opacity of the wireframes behind the digits.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
