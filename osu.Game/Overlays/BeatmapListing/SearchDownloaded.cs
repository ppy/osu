// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchDownloaded
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.DownloadedAny))]
        Any,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.DownloadedNotDownloaded))]
        NotDownloaded,
    }
}
