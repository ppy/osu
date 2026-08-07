// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mania
{
    public static class ManiaEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mania.ManiaEditor";

        /// <summary>
        /// "Note"
        /// </summary>
        public static LocalisableString NoteTool => new TranslatableString(getKey(@"note_tool"), @"Note");

        /// <summary>
        /// "Hold note"
        /// </summary>
        public static LocalisableString HoldNoteTool => new TranslatableString(getKey(@"hold_note_tool"), @"Hold note");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
