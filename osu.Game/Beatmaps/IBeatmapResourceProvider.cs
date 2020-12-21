// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapResourceProvider
    {
        /// <summary>
        /// Retrieve a global large texture store, used for loading beatmap backgrounds.
        /// </summary>
        TextureStore LargeTextureStore { get; }

        /// <summary>
        /// Access a global track store for retrieving beatmap tracks from.
        /// </summary>
        ITrackStore Tracks { get; }

        /// <summary>
        /// Retrieve the game-wide audio manager.
        /// </summary>
        AudioManager AudioManager { get; }

        /// <summary>
        /// Access game-wide user files.
        /// </summary>
        IResourceStore<byte[]> Files { get; }

        /// <summary>
        /// Create a texture loader store based on an underlying data store.
        /// </summary>
        /// <param name="underlyingStore">The underlying provider of texture data (in arbitrary image formats).</param>
        /// <returns>A texture loader store.</returns>
        IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore);
    }
}
