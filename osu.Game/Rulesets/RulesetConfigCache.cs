// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets
{
    public class RulesetConfigCache : Component, IRulesetConfigCache
    {
        private readonly RealmAccess realm;
        private readonly RulesetStore rulesets;

        private readonly Dictionary<string, IRulesetConfigManager> configCache = new Dictionary<string, IRulesetConfigManager>();

        public RulesetConfigCache(RealmAccess realm, RulesetStore rulesets)
        {
            this.realm = realm;
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var settingsStore = new SettingsStore(realm);

            // let's keep things simple for now and just retrieve all the required configs at startup..
            foreach (var ruleset in rulesets.AvailableRulesets)
            {
                if (string.IsNullOrEmpty(ruleset.ShortName))
                    continue;

                configCache[ruleset.ShortName] = ruleset.CreateInstance().CreateConfig(settingsStore);
            }
        }

        public IRulesetConfigManager GetConfigFor(Ruleset ruleset)
        {
            if (!IsLoaded)
                throw new InvalidOperationException($@"Cannot retrieve {nameof(IRulesetConfigManager)} before {nameof(RulesetConfigCache)} has loaded");

            if (!configCache.TryGetValue(ruleset.RulesetInfo.ShortName, out var config))
                throw new InvalidOperationException($@"Attempted to retrieve {nameof(IRulesetConfigManager)} for an unavailable ruleset {ruleset.GetDisplayString()}");

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
