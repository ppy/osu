// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DebugSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DebugSettings";

        /// <summary>
        /// "Debug"
        /// </summary>
        public static LocalisableString DebugSectionHeader => new TranslatableString(getKey(@"debug_section_header"), @"Debug");

        /// <summary>
        /// "Show log overlay"
        /// </summary>
        public static LocalisableString ShowLogOverlay => new TranslatableString(getKey(@"show_log_overlay"), @"Show log overlay");

        /// <summary>
        /// "Bypass front-to-back render pass"
        /// </summary>
        public static LocalisableString BypassFrontToBackPass => new TranslatableString(getKey(@"bypass_front_to_back_pass"), @"Bypass front-to-back render pass");

        /// <summary>
        /// "Import files"
        /// </summary>
        public static LocalisableString ImportFiles => new TranslatableString(getKey(@"import_files"), @"Import files");

        /// <summary>
        /// "Memory"
        /// </summary>
        public static LocalisableString MemoryHeader => new TranslatableString(getKey(@"memory_header"), @"Memory");

        /// <summary>
        /// "Clear all caches"
        /// </summary>
        public static LocalisableString ClearAllCaches => new TranslatableString(getKey(@"clear_all_caches"), @"Clear all caches");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
