// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Overlays.BeatmapListing.Panels;

namespace osu.Game.Online
{
    /// <summary>
    /// An interface used for components that may be backed with a beatmap containing explicit content.
    /// </summary>
    /// <remarks>
    /// See <see cref="BeatmapSetCover"/> and <see cref="PlayButton"/> for an example of usage.
    /// </remarks>
    public interface IExplicitContentPermission
    {
        /// <summary>
        /// Whether user allowed displaying explicit content on screen.
        /// </summary>
        IBindable<bool> UserAllowed { get; }
    }
}
