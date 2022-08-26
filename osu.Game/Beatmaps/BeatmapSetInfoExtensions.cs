// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;

namespace osu.Game.Beatmaps
{
    public static class BeatmapSetInfoExtensions
    {
        /// <summary>
        /// Returns the storage path for the file in this beatmapset with the given filename, if any exists, otherwise null.
        /// The path returned is relative to the user file storage.
        /// The lookup is case insensitive.
        /// </summary>
        /// <param name="model">The model to operate on.</param>
        /// <param name="filename">The name of the file to get the storage path of.</param>
        public static string? GetPathForFile(this IHasRealmFiles model, string filename) => model.GetFile(filename)?.File.GetStoragePath();

        /// <summary>
        /// Returns the file usage for the file in this beatmapset with the given filename, if any exists, otherwise null.
        /// The path returned is relative to the user file storage.
        /// The lookup is case insensitive.
        /// </summary>
        /// <param name="model">The model to operate on.</param>
        /// <param name="filename">The name of the file to get the storage path of.</param>
        public static RealmNamedFileUsage? GetFile(this IHasRealmFiles model, string filename) =>
            model.Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase));
    }
}
