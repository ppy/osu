// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets
{
    /// <summary>
    /// A cache that provides a single <see cref="IRulesetConfigManager"/> per-ruleset.
    /// This is done to support referring to and updating ruleset configs from multiple locations in the absence of inter-config bindings.
    /// </summary>
    public class RulesetConfigCache : Component
    {
        private readonly RealmContextFactory realmFactory;
        private readonly RulesetStore rulesets;

        private readonly Dictionary<int, IRulesetConfigManager> configCache = new Dictionary<int, IRulesetConfigManager>();

        public RulesetConfigCache(RealmContextFactory realmFactory, RulesetStore rulesets)
        {
            this.realmFactory = realmFactory;
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var settingsStore = new SettingsStore(realmFactory);

            // let's keep things simple for now and just retrieve all the required configs at startup..
            foreach (var ruleset in rulesets.AvailableRulesets)
            {
                if (ruleset.ID == null)
                    continue;

                configCache[ruleset.ID.Value] = ruleset.CreateInstance().CreateConfig(settingsStore);
            }
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

            if (!configCache.TryGetValue(ruleset.RulesetInfo.ID.Value, out var config))
                // any ruleset request which wasn't initialised on startup should not be stored to realm.
                // this should only be used by tests.
                return ruleset.CreateConfig(null);

            return config;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // ensures any potential database operations are finalised before game destruction.
            foreach (var c in configCache.Values)
                c?.Dispose();
        }
    }
}
