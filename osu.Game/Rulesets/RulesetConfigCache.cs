// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets
{
    /// <summary>
    /// A cache that provides a single <see cref="IRulesetConfigManager"/> per-ruleset.
    /// This is done to support referring to and updating ruleset configs from multiple locations in the absence of inter-config bindings.
    /// </summary>
    public class RulesetConfigCache : Component
    {
        private readonly ConcurrentDictionary<int, IRulesetConfigManager> configCache = new ConcurrentDictionary<int, IRulesetConfigManager>();
        private readonly SettingsStore settingsStore;

        public RulesetConfigCache(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;
        }

        /// <summary>
        /// Retrieves the <see cref="IRulesetConfigManager"/> for a <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="ruleset">The <see cref="Ruleset"/> to retrieve the <see cref="IRulesetConfigManager"/> for.</param>
        /// <returns>The <see cref="IRulesetConfigManager"/> defined by <paramref name="ruleset"/>, null if <paramref name="ruleset"/> doesn't define one.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="ruleset"/> doesn't have a valid <see cref="RulesetInfo.ID"/>.</exception>
        public IRulesetConfigManager GetConfigFor(Ruleset ruleset)
        {
            if (ruleset.RulesetInfo.ID == null)
                return null;

            return configCache.GetOrAdd(ruleset.RulesetInfo.ID.Value, _ => ruleset.CreateConfig(settingsStore));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // ensures any potential database operations are finalised before game destruction.
            foreach (var c in configCache.Values)
                (c as IDisposable)?.Dispose();
        }
    }
}
