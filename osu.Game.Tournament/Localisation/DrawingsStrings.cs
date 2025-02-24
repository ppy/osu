// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class DrawingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Drawings";

        /// <summary>
        /// "No drawings.txt file found. Please create one and restart the client."
        /// </summary>
        public static LocalisableString NoDrawingDataWarning => new TranslatableString(getKey(@"no_drawing_data_warning"),
            @"No drawings.txt file found. Please create one and restart the client.");

        /// <summary>
        /// "Click for details on the file format"
        /// </summary>
        public static LocalisableString FileFormatLinkText => new TranslatableString(getKey(@"file_format_link_text"), @"Click for details on the file format");

        /// <summary>
        /// "Begin Random"
        /// </summary>
        public static LocalisableString BeginRandom => new TranslatableString(getKey(@"begin_random"), @"Begin Random");

        /// <summary>
        /// "Stop Random"
        /// </summary>
        public static LocalisableString StopRandom => new TranslatableString(getKey(@"stop_random"), @"Stop Random");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
