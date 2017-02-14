using System;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public static class BeatmapFilter
    {
        public static void Filter(BeatmapMetadata beatmapMetadata, string search, Action matchAction, Action notMatchAction)
        {
            var match = string.IsNullOrEmpty(search)
                        || (beatmapMetadata.Artist ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (beatmapMetadata.ArtistUnicode ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (beatmapMetadata.Title ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (beatmapMetadata.TitleUnicode ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1;
            if (match)
                matchAction();
            else
                notMatchAction();
        }
    }
}
