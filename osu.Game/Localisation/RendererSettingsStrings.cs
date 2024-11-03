// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RendererSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RendererSettings";

        /// <summary>
        /// "Frame limiters are unavailable in this combination of renderer and threading mode."
        /// </summary>
        public static LocalisableString FrameLimitersUnavailableTooltip => new TranslatableString(getKey(@"frame_limiters_unavailable_tooltip"), @"Frame limiters are unavailable in this combination of renderer and threading mode.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
