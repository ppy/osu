// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An abstract skin implementation, whose primary purpose is to properly handle component fallback across multiple layers of skins (e.g.: beatmap skin, user skin, default skin).
    /// </summary>
    /// <remarks>
    /// Common usage is to do an initial lookup via <see cref="FindProvider"/>, and use the returned <see cref="ISkin"/>
    /// to do further lookups for related components.
    ///
    /// The initial lookup is used to lock consecutive lookups to the same underlying skin source (as to not get some elements
    /// from one skin and others from another, which would be the case if using <see cref="ISkin"/> methods like
    /// <see cref="ISkin.GetSample"/> directly).
    /// </remarks>
    public interface ISkinSource : ISkin
    {
        /// <summary>
        /// Fired whenever a source change occurs, signalling that consumers should re-query as required.
        /// </summary>
        event Action SourceChanged;

        /// <summary>
        /// Find the first (if any) skin that can fulfill the lookup.
        /// This should be used for cases where subsequent lookups (for related components) need to occur on the same skin.
        /// </summary>
        /// <returns>The skin to be used for subsequent lookups, or <c>null</c> if none is available.</returns>
        ISkin? FindProvider(Func<ISkin, bool> lookupFunction);

        /// <summary>
        /// Retrieve all sources available for lookup, with highest priority source first.
        /// </summary>
        IEnumerable<ISkin> AllSources { get; }
    }
}
