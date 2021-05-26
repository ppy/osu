// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.IO;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapResourceProvider : IStorageResourceProvider
    {
        /// <summary>
        /// Retrieve a global large texture store, used for loading beatmap backgrounds.
        /// </summary>
        TextureStore LargeTextureStore { get; }

        /// <summary>
        /// Access a global track store for retrieving beatmap tracks from.
        /// </summary>
        ITrackStore Tracks { get; }
    }
}
