// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Models;

namespace osu.Game.Database
{
    /// <summary>
    /// A model that contains a list of files it is responsible for.
    /// </summary>
    public interface IHasRealmFiles
    {
        /// <summary>
        /// Available files in this model, with locally filenames.
        /// When performing lookups, consider using <see cref="BeatmapSetInfoExtensions.GetFile"/> or <see cref="BeatmapSetInfoExtensions.GetPathForFile"/> to do case-insensitive lookups.
        /// </summary>
        IList<RealmNamedFileUsage> Files { get; }

        /// <summary>
        /// A combined hash representing the model, based on the files it contains.
        /// Implementation specific.
        /// </summary>
        string Hash { get; set; }
    }
}
