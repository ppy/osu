// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets
{
    /// <summary>
    /// A cache that provides a single <see cref="IRulesetConfigManager"/> per-ruleset.
    /// This is done to support referring to and updating ruleset configs from multiple locations in the absence of inter-config bindings.
    /// </summary>
    public interface IRulesetConfigCache
    {
        /// <summary>
        /// Retrieves the <see cref="IRulesetConfigManager"/> for a <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="ruleset">The <see cref="Ruleset"/> to retrieve the <see cref="IRulesetConfigManager"/> for.</param>
        /// <returns>The <see cref="IRulesetConfigManager"/> defined by <paramref name="ruleset"/>, null if <paramref name="ruleset"/> doesn't define one.</returns>
        public IRulesetConfigManager? GetConfigFor(Ruleset ruleset);
    }
}
