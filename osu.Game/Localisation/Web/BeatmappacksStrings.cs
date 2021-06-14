// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmappacksStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Beatmappacks";

        /// <summary>
        /// "Pre-packaged collections of beatmaps based around a common theme."
        /// </summary>
        public static LocalisableString IndexDescription => new TranslatableString(getKey(@"index.description"), @"Pre-packaged collections of beatmaps based around a common theme.");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString IndexNavTitle => new TranslatableString(getKey(@"index.nav_title"), @"listing");

        /// <summary>
        /// "Beatmap Packs"
        /// </summary>
        public static LocalisableString IndexTitle => new TranslatableString(getKey(@"index.title"), @"Beatmap Packs");

        /// <summary>
        /// "READ THIS BEFORE DOWNLOADING"
        /// </summary>
        public static LocalisableString IndexBlurbImportant => new TranslatableString(getKey(@"index.blurb.important"), @"READ THIS BEFORE DOWNLOADING");

        /// <summary>
        /// "Installation: Once a pack has been downloaded, extract the .rar into your osu! Songs directory.
        ///                     All songs are still .zip&#39;d and/or .osz&#39;d inside the pack, so osu! will need to extract the beatmaps itself the next time you go into Play mode.
        ///                     {0} extract the zip&#39;s/osz&#39;s yourself,
        ///                     or the beatmaps will display incorrectly in osu! and will not function properly."
        /// </summary>
        public static LocalisableString IndexBlurbInstructionDefault(string scary) => new TranslatableString(getKey(@"index.blurb.instruction._"), @"Installation: Once a pack has been downloaded, extract the .rar into your osu! Songs directory.
                    All songs are still .zip'd and/or .osz'd inside the pack, so osu! will need to extract the beatmaps itself the next time you go into Play mode.
                    {0} extract the zip's/osz's yourself,
                    or the beatmaps will display incorrectly in osu! and will not function properly.", scary);

        /// <summary>
        /// "Do NOT"
        /// </summary>
        public static LocalisableString IndexBlurbInstructionScary => new TranslatableString(getKey(@"index.blurb.instruction.scary"), @"Do NOT");

        /// <summary>
        /// "Also note that it is highly recommended to {0}, since the oldest maps are of much lower quality than most recent maps."
        /// </summary>
        public static LocalisableString IndexBlurbNoteDefault(string scary) => new TranslatableString(getKey(@"index.blurb.note._"), @"Also note that it is highly recommended to {0}, since the oldest maps are of much lower quality than most recent maps.", scary);

        /// <summary>
        /// "download the packs from latest to earliest"
        /// </summary>
        public static LocalisableString IndexBlurbNoteScary => new TranslatableString(getKey(@"index.blurb.note.scary"), @"download the packs from latest to earliest");

        /// <summary>
        /// "Download"
        /// </summary>
        public static LocalisableString ShowDownload => new TranslatableString(getKey(@"show.download"), @"Download");

        /// <summary>
        /// "cleared"
        /// </summary>
        public static LocalisableString ShowItemCleared => new TranslatableString(getKey(@"show.item.cleared"), @"cleared");

        /// <summary>
        /// "not cleared"
        /// </summary>
        public static LocalisableString ShowItemNotCleared => new TranslatableString(getKey(@"show.item.not_cleared"), @"not cleared");

        /// <summary>
        /// "{0} may not be used to clear this pack."
        /// </summary>
        public static LocalisableString ShowNoDiffReductionDefault(string link) => new TranslatableString(getKey(@"show.no_diff_reduction._"), @"{0} may not be used to clear this pack.", link);

        /// <summary>
        /// "Difficulty reduction mods"
        /// </summary>
        public static LocalisableString ShowNoDiffReductionLink => new TranslatableString(getKey(@"show.no_diff_reduction.link"), @"Difficulty reduction mods");

        /// <summary>
        /// "Artist/Album"
        /// </summary>
        public static LocalisableString ModeArtist => new TranslatableString(getKey(@"mode.artist"), @"Artist/Album");

        /// <summary>
        /// "Spotlights"
        /// </summary>
        public static LocalisableString ModeChart => new TranslatableString(getKey(@"mode.chart"), @"Spotlights");

        /// <summary>
        /// "Standard"
        /// </summary>
        public static LocalisableString ModeStandard => new TranslatableString(getKey(@"mode.standard"), @"Standard");

        /// <summary>
        /// "Theme"
        /// </summary>
        public static LocalisableString ModeTheme => new TranslatableString(getKey(@"mode.theme"), @"Theme");

        /// <summary>
        /// "You need to be {0} to download"
        /// </summary>
        public static LocalisableString RequireLoginDefault(string link) => new TranslatableString(getKey(@"require_login._"), @"You need to be {0} to download", link);

        /// <summary>
        /// "signed in"
        /// </summary>
        public static LocalisableString RequireLoginLinkText => new TranslatableString(getKey(@"require_login.link_text"), @"signed in");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}