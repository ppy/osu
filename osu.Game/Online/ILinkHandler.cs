// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.Chat;

namespace osu.Game.Online
{
    /// <summary>
    /// Handle an arbitrary URL. Displays via in-game overlays where possible.
    /// Methods can be called from a non-thread-safe non-game-loaded state.
    /// </summary>
    [Cached]
    public interface ILinkHandler
    {
        /// <summary>
        /// Handle an arbitrary URL. Displays via in-game overlays where possible.
        /// This can be called from a non-thread-safe non-game-loaded state.
        /// </summary>
        /// <param name="url">The URL to load.</param>
        void HandleLink(string url);

        /// <summary>
        /// Handle a specific <see cref="LinkDetails"/>.
        /// This can be called from a non-thread-safe non-game-loaded state.
        /// </summary>
        /// <param name="link">The link to load.</param>
        void HandleLink(LinkDetails link);
    }
}
