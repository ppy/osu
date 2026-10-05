// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Scoring
{
    /// <summary>
    /// Contains all information about the configuration of a mod in an <see cref="IScoreInfo"/>.
    /// </summary>
    /// <remarks>
    /// Note the difference between this interface and <see cref="IMod"/>.
    /// <see cref="IMod"/> represents a fully-working instance of a mod with mutable settings and full metadata for a given mod.
    /// <see cref="IConfiguredMod"/> only contains the information necessary to reconstruct a full <see cref="IMod"/> instance
    /// configured in a way that matches a recorded score.
    /// </remarks>
    public interface IConfiguredMod
    {
        /// <summary>
        /// The acronym of the mod.
        /// </summary>
        string Acronym { get; }

        /// <summary>
        /// The settings used with the mod.
        /// </summary>
        IReadOnlyDictionary<string, object> Settings { get; }
    }
}
