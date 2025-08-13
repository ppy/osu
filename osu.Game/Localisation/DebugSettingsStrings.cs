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
        /// "Memory"
        /// </summary>
        public static LocalisableString MemoryHeader => new TranslatableString(getKey(@"memory_header"), @"Memory");

        /// <summary>
        /// "Clear all caches"
        /// </summary>
        public static LocalisableString ClearAllCaches => new TranslatableString(getKey(@"clear_all_caches"), @"Clear all caches");

        /// <summary>
        /// "GC mode"
        /// </summary>
        public static LocalisableString GarbageCollectorMode => new TranslatableString(getKey(@"garbage_collector_mode"), @"GC mode");

        /// <summary>
        /// "Interactive"
        /// </summary>
        public static LocalisableString Interactive => new TranslatableString(getKey(@"interactive"), @"Interactive");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
