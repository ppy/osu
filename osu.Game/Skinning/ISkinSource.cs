﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides access to skinnable elements.
    /// </summary>
    public interface ISkinSource : ISkin
    {
        event Action SourceChanged;

        /// <summary>
        /// Find the first (if any) skin that can fulfill the lookup.
        /// This should be used for cases where subsequent lookups (for related components) need to occur on the same skin.
        /// </summary>
        /// <returns>The skin to be used for subsequent lookups, or <c>null</c> if none is available.</returns>
        [CanBeNull]
        ISkin FindProvider(Func<ISkin, bool> lookupFunction);
    }
}
