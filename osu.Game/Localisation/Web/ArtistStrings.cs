// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ArtistStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Artist";

        /// <summary>
        /// "Featured artists on osu!"
        /// </summary>
        public static LocalisableString PageDescription => new TranslatableString(getKey(@"page_description"), @"Featured artists on osu!");

        /// <summary>
        /// "Featured Artists"
        /// </summary>
        public static LocalisableString Title => new TranslatableString(getKey(@"title"), @"Featured Artists");

        /// <summary>
        /// "ARTIST IS CURRENTLY HIDDEN"
        /// </summary>
        public static LocalisableString AdminHidden => new TranslatableString(getKey(@"admin.hidden"), @"ARTIST IS CURRENTLY HIDDEN");

        /// <summary>
        /// "Beatmaps"
        /// </summary>
        public static LocalisableString BeatmapsDefault => new TranslatableString(getKey(@"beatmaps._"), @"Beatmaps");

        /// <summary>
        /// "Download Beatmap Template"
        /// </summary>
        public static LocalisableString BeatmapsDownload => new TranslatableString(getKey(@"beatmaps.download"), @"Download Beatmap Template");

        /// <summary>
        /// "Beatmap Template not yet available"
        /// </summary>
        public static LocalisableString BeatmapsDownloadNa => new TranslatableString(getKey(@"beatmaps.download-na"), @"Beatmap Template not yet available");

        /// <summary>
        /// "Featured artists are artists that we are working in collaboration with in order to bring new and original music to osu!. These artists and a selection of their tracks have been hand-picked by the osu! team as being awesomesauce and suitable for mapping. Some of these featured artists have also created exclusive new tracks for use in osu!.&lt;br&gt;&lt;br&gt;All tracks in this section are provided as pre-timed .osz files and have been officially licensed for use in osu! and osu!-related content."
        /// </summary>
        public static LocalisableString IndexDescription => new TranslatableString(getKey(@"index.description"), @"Featured artists are artists that we are working in collaboration with in order to bring new and original music to osu!. These artists and a selection of their tracks have been hand-picked by the osu! team as being awesomesauce and suitable for mapping. Some of these featured artists have also created exclusive new tracks for use in osu!.<br><br>All tracks in this section are provided as pre-timed .osz files and have been officially licensed for use in osu! and osu!-related content.");

        /// <summary>
        /// "osu! profile"
        /// </summary>
        public static LocalisableString LinksOsu => new TranslatableString(getKey(@"links.osu"), @"osu! profile");

        /// <summary>
        /// "Official Website"
        /// </summary>
        public static LocalisableString LinksSite => new TranslatableString(getKey(@"links.site"), @"Official Website");

        /// <summary>
        /// "Songs"
        /// </summary>
        public static LocalisableString SongsDefault => new TranslatableString(getKey(@"songs._"), @"Songs");

        /// <summary>
        /// "{0} song|{0} songs"
        /// </summary>
        public static LocalisableString SongsCount(string countDelimited) => new TranslatableString(getKey(@"songs.count"), @"{0} song|{0} songs", countDelimited);

        /// <summary>
        /// "osu! exclusive"
        /// </summary>
        public static LocalisableString SongsExclusive => new TranslatableString(getKey(@"songs.exclusive"), @"osu! exclusive");

        /// <summary>
        /// "title"
        /// </summary>
        public static LocalisableString TracklistTitle => new TranslatableString(getKey(@"tracklist.title"), @"title");

        /// <summary>
        /// "length"
        /// </summary>
        public static LocalisableString TracklistLength => new TranslatableString(getKey(@"tracklist.length"), @"length");

        /// <summary>
        /// "bpm"
        /// </summary>
        public static LocalisableString TracklistBpm => new TranslatableString(getKey(@"tracklist.bpm"), @"bpm");

        /// <summary>
        /// "genre"
        /// </summary>
        public static LocalisableString TracklistGenre => new TranslatableString(getKey(@"tracklist.genre"), @"genre");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}