// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class CinemaModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.CinemaMod";

        /// <summary>
        /// "Watch the video without visual distractions."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Watch the video without visual distractions.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
