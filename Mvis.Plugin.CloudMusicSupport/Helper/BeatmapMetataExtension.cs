// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public static class BeatmapMetataExtension
    {
        public static string GetTitle(this BeatmapMetadata metadata)
        {
            return string.IsNullOrEmpty(metadata.TitleUnicode)
                ? metadata.Title
                : metadata.TitleUnicode;
        }

        public static string GetArtist(this BeatmapMetadata metadata)
        {
            return string.IsNullOrEmpty(metadata.ArtistUnicode)
                ? metadata.Artist
                : metadata.ArtistUnicode;
        }
    }
}
