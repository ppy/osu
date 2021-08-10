// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BeatmapDownloaderStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapDownloader";

        /// <summary>
        /// "You need to be logged in to download Beatmaps"
        /// </summary>
        public static LocalisableString YouNeedToBeLoggedInToDownloadBeatmaps => new TranslatableString(getKey(@"you_need_to_be_logged_in_to_download_beatmaps"), @"You need to be logged in to download Beatmaps");

        /// <summary>
        /// "The Last Downloaded Beatmap Time is in the Future"
        /// </summary>
        public static LocalisableString TheLastDownloadedBeatmapTimeIsInTheFuture => new TranslatableString(getKey(@"the_last_downloaded_beatmap_time_is_in_the_future"), @"The Last Downloaded Beatmap Time is in the Future");

        /// <summary>
        /// "Please wait a Minute before requesting new Beatmaps"
        /// </summary>
        public static LocalisableString PleaseWaitAMinuteBeforeRequestingNewBeatmaps => new TranslatableString(getKey(@"please_wait_a_minute_before_requesting_new_beatmaps"), @"Please wait a Minute before requesting new Beatmaps");

        /// <summary>
        /// "Finished downloading new Beatmaps"
        /// </summary>
        public static LocalisableString FinishedDownloadingNewBeatmaps => new TranslatableString(getKey(@"finished_downloading_new_beatmaps"), @"Finished downloading new Beatmaps");

        /// <summary>
        /// "An Error has occured while downloading the Beatmaps: {0}"
        /// </summary>
        public static LocalisableString AnErrorHasOccuredWhileDownloadingTheBeatmaps(string arg0) => new TranslatableString(getKey(@"an_error_has_occured_while_downloading_the_beatmaps"), @"An Error has occured while downloading the Beatmaps: {0}", arg0);

        /// <summary>
        /// "An internal Error has occured while downloading the Beatmaps: {0}"
        /// </summary>
        public static LocalisableString AnInternalErrorHasOccuredWhileDownloadingTheBeatmaps(string arg0) => new TranslatableString(getKey(@"an_internal_error_has_occured_while_downloading_the_beatmaps"), @"An internal Error has occured while downloading the Beatmaps: {0}", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
